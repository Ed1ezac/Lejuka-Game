using System;
using System.IO;
using System.Threading;
using Lejuka.Contracts.Players;
using Lejuka.Unity.Persistence;
using NUnit.Framework;

namespace Lejuka.Tests.UnityEditMode.Persistence
{
    public sealed class JsonProfileRepositoryTests
    {
        private const string ProfileId = "local-player";

        private string _folder;
        private string _filePath;

        [SetUp]
        public void SetUp()
        {
            // The folder is deliberately not created: saving has to create it.
            _folder = Path.Combine(Path.GetTempPath(), "lejuka-tests-" + Guid.NewGuid().ToString("N"));
            _filePath = Path.Combine(_folder, JsonProfileRepository.DefaultFileName);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_folder))
            {
                Directory.Delete(_folder, true);
            }
        }

        [Test]
        public void Load_ReturnsNull_WhenNothingIsSaved()
        {
            var repository = new JsonProfileRepository(_filePath);

            var loaded = repository.LoadAsync(ProfileId, CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(loaded, Is.Null);
        }

        [Test]
        public void SaveThenLoad_RoundTripsEveryField()
        {
            var saved = new PlayerProfileData(ProfileId, "Thabo", 37, 9, 4, 21);

            new JsonProfileRepository(_filePath).SaveAsync(saved, CancellationToken.None).GetAwaiter().GetResult();

            // A second repository, so the data must have come from the file.
            var loaded = new JsonProfileRepository(_filePath)
                .LoadAsync(ProfileId, CancellationToken.None)
                .GetAwaiter()
                .GetResult();

            Assert.That(loaded, Is.EqualTo(saved));
        }

        [Test]
        public void Save_ReplacesThePreviousVersion_AndLeavesNoTempFile()
        {
            var repository = new JsonProfileRepository(_filePath);
            repository.SaveAsync(new PlayerProfileData(ProfileId, "Thabo", 20, 0, 0, 0), CancellationToken.None)
                .GetAwaiter()
                .GetResult();
            repository.SaveAsync(new PlayerProfileData(ProfileId, "Thabo", 22, 1, 1, 2), CancellationToken.None)
                .GetAwaiter()
                .GetResult();

            var loaded = repository.LoadAsync(ProfileId, CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(loaded.Credits, Is.EqualTo(22));
            Assert.That(loaded.MatchesPlayed, Is.EqualTo(1));
            Assert.That(Directory.GetFiles(_folder).Length, Is.EqualTo(1));
        }

        [Test]
        public void Load_ReturnsNull_ForADifferentId()
        {
            var repository = new JsonProfileRepository(_filePath);
            repository.SaveAsync(new PlayerProfileData(ProfileId, "Thabo", 20, 0, 0, 0), CancellationToken.None)
                .GetAwaiter()
                .GetResult();

            var loaded = repository.LoadAsync("someone-else", CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(loaded, Is.Null);
        }

        [Test]
        public void Delete_RemovesTheProfile_AndIsSafeWhenThereIsNone()
        {
            var repository = new JsonProfileRepository(_filePath);
            Assert.DoesNotThrow(() => repository.Delete());

            repository.SaveAsync(new PlayerProfileData(ProfileId, "Thabo", 20, 0, 0, 0), CancellationToken.None)
                .GetAwaiter()
                .GetResult();
            repository.Delete();

            Assert.That(repository.LoadAsync(ProfileId, CancellationToken.None).GetAwaiter().GetResult(), Is.Null);
        }

        [Test]
        public void CancelledToken_CancelsLoadAndSave()
        {
            var repository = new JsonProfileRepository(_filePath);
            using (var cancellation = new CancellationTokenSource())
            {
                cancellation.Cancel();

                Assert.That(repository.LoadAsync(ProfileId, cancellation.Token).IsCanceled, Is.True);
                Assert.That(
                    repository.SaveAsync(new PlayerProfileData(ProfileId, "Thabo", 1, 0, 0, 0), cancellation.Token).IsCanceled,
                    Is.True);
                Assert.That(File.Exists(_filePath), Is.False);
            }
        }
    }
}
