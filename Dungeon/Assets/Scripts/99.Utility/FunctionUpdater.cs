using System;
using UnityEngine;
using System.Collections.Generic;

public class FunctionUpdater {

    // MonoBehaviourHook : MonoBehaviour에서 Update를 호출할 수 있는 래퍼 클래스
    private class MonoBehaviourHook : MonoBehaviour {
        public Action OnUpdate;
        private void Update() {
            OnUpdate?.Invoke();
        }
    }

    private static List<FunctionUpdater> updaterList; // 활성화된 모든 FunctionUpdater의 목록
    private static GameObject initGameObject; // 클래스 초기화를 위한 전역 GameObject

    // 초기화 메서드
    private static void InitIfNeeded() {
        if (initGameObject == null) {
            initGameObject = new GameObject("FunctionUpdater_Global");
            updaterList = new List<FunctionUpdater>();
        }
    }

    // FunctionUpdater 생성 메서드 오버로드
    public static FunctionUpdater Create(Action updateFunc) {
        return Create(() => { updateFunc(); return false; });
    }
    public static FunctionUpdater Create(Func<bool> updateFunc) {
        return Create(updateFunc, "", true, false);
    }
    public static FunctionUpdater Create(Func<bool> updateFunc, string functionName, bool active = true, bool stopAllWithSameName = false) {
        InitIfNeeded();

        if (stopAllWithSameName) {
            StopAllUpdatersWithName(functionName);
        }

        // 새로운 FunctionUpdater 생성 및 설정
        GameObject gameObject = new GameObject("FunctionUpdater Object " + functionName, typeof(MonoBehaviourHook));
        var functionUpdater = new FunctionUpdater(gameObject, updateFunc, functionName, active);
        gameObject.GetComponent<MonoBehaviourHook>().OnUpdate = functionUpdater.Update;

        updaterList.Add(functionUpdater);
        return functionUpdater;
    }

    // FunctionUpdater 제거 메서드
    private static void RemoveUpdater(FunctionUpdater funcUpdater) {
        InitIfNeeded();
        updaterList.Remove(funcUpdater);
    }

    // 특정 FunctionUpdater 파괴
    public static void DestroyUpdater(FunctionUpdater funcUpdater) {
        InitIfNeeded();
        funcUpdater?.DestroySelf();
    }

    // 이름으로 특정 FunctionUpdater 중지
    public static void StopUpdaterWithName(string functionName) {
        InitIfNeeded();
        for (int i = 0; i < updaterList.Count; i++) {
            if (updaterList[i].functionName == functionName) {
                updaterList[i].DestroySelf();
                return;
            }
        }
    }

    // 같은 이름의 모든 FunctionUpdater 중지
    public static void StopAllUpdatersWithName(string functionName) {
        InitIfNeeded();
        for (int i = 0; i < updaterList.Count; i++) {
            if (updaterList[i].functionName == functionName) {
                updaterList[i].DestroySelf();
                i--;
            }
        }
    }

    // 인스턴스 변수
    private GameObject gameObject;
    private string functionName;
    private bool active;
    private Func<bool> updateFunc; // true 반환 시 파괴

    // 생성자
    private FunctionUpdater(GameObject gameObject, Func<bool> updateFunc, string functionName, bool active) {
        this.gameObject = gameObject;
        this.updateFunc = updateFunc;
        this.functionName = functionName;
        this.active = active;
    }

    // 일시 중지 및 재개 메서드
    public void Pause() {
        active = false;
    }
    public void Resume() {
        active = true;
    }

    // 업데이트 메서드
    private void Update() {
        if (!active) return;
        if (updateFunc()) {
            DestroySelf();
        }
    }

    // Self 제거 메서드
    private void DestroySelf() {
        RemoveUpdater(this);
        if (gameObject != null) {
            UnityEngine.Object.Destroy(gameObject);
        }
    }
}
