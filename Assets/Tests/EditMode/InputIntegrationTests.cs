using System.IO;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Статическая диагностика ввода: какие UI Input Module реально
/// записаны в сцены и какой режим ввода включён в проекте.
///
/// Это не проверка Android touch. Она подтверждает только состав
/// EventSystem в сценах и отсутствие конфликтующего StandaloneInputModule.
/// </summary>
public class InputIntegrationTests
{
    [Test]
    public void Project_UsesInputSystemOnly()
    {
        string settings = File.ReadAllText(
            Path.Combine(
                Directory.GetCurrentDirectory(),
                "ProjectSettings",
                "ProjectSettings.asset"));

        StringAssert.Contains(
            "activeInputHandler: 1",
            settings,
            "Ожидается включённый Input System (activeInputHandler: 1).");
    }

    [Test]
    public void Scenes_UseInputSystemUIInputModuleOnly()
    {
        string[] scenes =
        {
            "MainMenu.unity",
            "MainRoad.unity",
            "Equipment.unity"
        };

        foreach (string scene in scenes)
        {
            string path = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Assets",
                "Scenes",
                scene);

            Assert.IsTrue(
                File.Exists(path),
                "Сцена не найдена: " + path);

            string text = File.ReadAllText(path);

            StringAssert.Contains(
                "InputSystemUIInputModule",
                text,
                scene + ": ожидается InputSystemUIInputModule.");

            StringAssert.DoesNotContain(
                "StandaloneInputModule",
                text,
                scene + ": не должно быть StandaloneInputModule.");
        }
    }
}
