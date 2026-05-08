
using System.Collections.Generic;

namespace YG
{
    [System.Serializable]
    public class SavesYG
    {
        // "Технические сохранения" для работы плагина (Не удалять)
        public int idSave;
        public bool isFirstSession = true;
        public string language = "ru";
        public bool promptDone;

        // Тестовые сохранения для демо сцены
        // Можно удалить этот код, но тогда удалите и демо (папка Example)
        //public int money = 1;                       // Можно задать полям значения по умолчанию
        public string newPlayerName = "Hello!";
        public bool[] openLevels = new bool[3];

        // Ваши сохранения

        // ====== МОИ ПОЛЯ ======
        public int money = 0;

        public int currentSkinID = 0;
        public List<int> purchasedSkins = new List<int>();

        public int currentHeadSkinID = 0;
        public List<int> purchasedHeadSkins = new List<int>();

        public int currentBlockSkinID = 0;
        public List<int> purchasedBlockSkins = new List<int>();

        public int levelIndex = 1;

        public int incomeLevel = 1;
        public int luckLevel = 1;

        public int fortuneSpin = 0;
        public int fortuneWheelCounter = 0;

        public string LastSavedDate = "";
        public int LastSavedStreak = 0;
        // ...

        // Поля (сохранения) можно удалять и создавать новые. При обновлении игры сохранения ломаться не должны


        // Вы можете выполнить какие то действия при загрузке сохранений
        //public SavesYG()
        //{
        //    // Допустим, задать значения по умолчанию для отдельных элементов массива

        //    openLevels[1] = true;
        //}
    }
}
