namespace MediumMvc.Data
{
    public class Seeder
    {
        private readonly AuthorSeeder _authorSeeder;
        private readonly PostSeeder _postSeeder;
        private readonly UserSeeder _userSeeder;

        public Seeder(
            AuthorSeeder authorSeeder,
            PostSeeder postSeeder,
            UserSeeder userSeeder)
        {
            _authorSeeder = authorSeeder;
            _postSeeder = postSeeder;
            _userSeeder = userSeeder;
        }

        public async Task SeedAsync()
        {
            await _authorSeeder.SeedAsync();
            await _postSeeder.SeedAsync();
            await _userSeeder.SeedAsync();
        }
    }
}
