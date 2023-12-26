namespace BattleCity.Common
{
    /// <summary>
    /// Информация об уничтоженном вражеском юните
    /// </summary>
    public class DestroyedEnemyInfo
    {
        /// <summary>
        /// Идентификатор юнита
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор текстуры (иконки)
        /// </summary>
        public int TextureId { get; set; }
    }
}
