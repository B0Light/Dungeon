using UnityEngine;
using System;
using System.IO;

public class SaveFileDataWriter
{
    public string saveDataDirectoryPath = "";
    public string saveFileName = "";

    // 새로운 저장 파일을 생성하기 전에, 해당 캐릭터 슬롯이 이미 존재하는지 확인해야 합니다
    public bool CheckToSeeIfFileExists()
    {
        if(File.Exists(Path.Combine(saveDataDirectoryPath, saveFileName)))
        {
            //Debug.LogWarning("YES DATA");
            return true;
        }
        else
        {
            //Debug.LogWarning("NO DATA");
            return false;
        }
    }

    // 캐릭터 저장 파일을 삭제하는 데 사용됩니다
    public void DeleteSaveFile()
    {
        File.Delete(Path.Combine(saveDataDirectoryPath, saveFileName));
    }

    // 새로운 게임을 시작할 때 저장 파일을 생성하는 데 사용됩니다
    public void CreateNewCharacterSaveFile(SaveGameData gameData)
    {
        // 파일을 저장할 경로 생성 (컴퓨터의 특정 위치)
        string savePath = Path.Combine(saveDataDirectoryPath, saveFileName);

        try
        {
            // 파일이 기록될 디렉터리가 존재하지 않으면 생성합니다
            Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            Debug.Log($"저장 파일 생성 중, 저장 경로: {savePath}");

            // C# 게임 데이터 객체를 JSON으로 직렬화합니다
            gameData.PrepareForSerialization();
            string dataToStore = JsonUtility.ToJson(gameData, true);

            // 파일을 시스템에 작성합니다
            using (FileStream stream = new FileStream(savePath, FileMode.Create))
            {
                using (StreamWriter fileWriter = new StreamWriter(stream))
                {
                    fileWriter.Write(dataToStore);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"캐릭터 데이터를 저장하는 중 오류 발생, 게임이 저장되지 않았습니다: {savePath} \n {ex}");
        }
    }

    // 이전 게임을 로드할 때 저장 파일을 로드하는 데 사용됩니다
    public SaveGameData LoadSaveFile()
    {
        SaveGameData gameData = null;

        // 파일을 로드할 경로 생성 (컴퓨터의 특정 위치)
        string loadPath = Path.Combine(saveDataDirectoryPath, saveFileName);

        if(File.Exists(loadPath))
        {
            try
            {
                string dataToLoad = "";

                using (FileStream stream = new FileStream(loadPath, FileMode.Open))
                {
                    using (StreamReader fileReader = new StreamReader(stream))
                    {
                        dataToLoad = fileReader.ReadToEnd();
                    }
                }

                // JSON 데이터를 Unity C# 객체로 역직렬화합니다
                gameData = JsonUtility.FromJson<SaveGameData>(dataToLoad);
                gameData.RestoreFromSerialization();
            }
            catch(Exception ex)
            {
                Debug.LogError($"캐릭터 데이터를 로드하는 중 오류 발생, 게임 데이터가 동기화되지 않았습니다: {loadPath} \n {ex}");
            }
        }

        return gameData;
    }
}
