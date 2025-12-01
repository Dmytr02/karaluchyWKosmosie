using System.Collections.Generic;
using UnityEngine; // Используем Random из Unity для лучшей совместимости

public static class ListExtensions
{
    // Метод расширения для перемешивания любого списка типа T
    public static void Shuffle<T>(this List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            // Используем UnityEngine.Random.Range для генерации случайного индекса
            // Внимание: Random.Range для int возвращает значения до (не включая) второго параметра
            // Поэтому используем (n + 1), чтобы включить текущий индекс n
            int k = Random.Range(0, n + 1); 
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}