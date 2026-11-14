using UnityEngine;

/// <summary>
/// Игра обязана продолжать работать, когда окно Unity теряет фокус
/// (переключение в IDE, чат, другой монитор).
///
/// Без этого player loop замирает, пока окно не в фокусе: первый клик
/// по кнопке лишь возвращает окну фокус и размораживает игру, а
/// срабатывает уже только второй клик — «кнопки нажимаются со второго
/// раза» в главном меню и во всех разделах.
///
/// На мобильных устройствах ОС сама уводит приложение в паузу при
/// сворачивании, так что на геймплей на телефоне это не влияет.
/// </summary>
public static class AppRunInBackground
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    private static void EnableRunInBackground()
    {
        Application.runInBackground = true;
    }
}
