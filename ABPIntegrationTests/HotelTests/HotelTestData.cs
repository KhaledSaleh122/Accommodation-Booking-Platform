namespace ABPIntegrationTests.HotelTests
{
    public static class HotelTestData
    {
        public static IEnumerable<object[]> NotFoundTestData =>
            new List<object[]>
            {
                new object[] { 99999 },
                new object[] { -1 },
                new object[] { int.MaxValue },
                new object[] { "test" },
            };
    }
}
