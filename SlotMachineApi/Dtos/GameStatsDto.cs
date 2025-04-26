namespace SlotMachineApi.DTOs
{
    public class GameStatsDto
    {
        public int TotalPlayers { get; set; }
        public int TotalGames { get; set; }
        public int TotalWins { get; set; }
        public double WinRate { get; set; }
    }
}