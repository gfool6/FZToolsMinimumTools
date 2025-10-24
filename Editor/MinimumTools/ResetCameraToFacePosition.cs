using System.Security.AccessControl;
using System.Diagnostics;
using System;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDK3.Avatars.Components;
using EUI = FZTools.EditorUtils.UI;
using ELayout = FZTools.EditorUtils.Layout;
using static FZTools.FZToolsConstants;

namespace FZTools
{
    [InitializeOnLoad]
    public static class ResetCameraToFacePosition
    {
        static Scene scene;
        static Camera previewCamera;

        static ResetCameraToFacePosition()
        {
            EditorApplication.delayCall += () =>
            {
                var gameview = EditorWindow.GetWindow(GetGameView());

                var toolbar = new VisualElement();
                var style = toolbar.style;
                var rootVisualElement = gameview.rootVisualElement;

                style.flexDirection = FlexDirection.Row;
                style.top = 20;
                style.height = 20;

                toolbar.Add(new Button(() => Preview()) { text = "顔プレビューカメラに切り替え" });
                toolbar.Add(new Button(() => SwitchMain()) { text = "メインカメラに戻す" });
                toolbar.BringToFront();

                rootVisualElement.Clear();
                rootVisualElement.Add(toolbar);

                scene = EditorSceneManager.GetActiveScene();
            };
        }

        static Type GetGameView()
        {
            var assembly = typeof(EditorWindow).Assembly;
            var type = assembly.GetType("UnityEditor.PlayModeView");
            return type;
        }

        static FieldInfo GetTargetDisplay(Type gameView)
        {
            return gameView.GetField("m_TargetDisplay", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        static EditorWindow GetEditorWindow(Type gameView)
        {
            return EditorWindow.GetWindow(gameView);
        }

        static void SwitchMain()
        {
            if (previewCamera != null)
            {
                var gameView = GetGameView();
                GetTargetDisplay(gameView).SetValue(GetEditorWindow(gameView), 0);
            }
        }

        static void Preview()
        {
            if (previewCamera == null)
            {
                CreatePreviewCamera();
            }
            var enabledAvatarDescriptor = GameObject.FindObjectsOfType<VRCAvatarDescriptor>().Last();
            var headBone = enabledAvatarDescriptor.gameObject.GetBoneRootObject().GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name.ToLower().Contains("head"));
            var headPosition = headBone.position + new Vector3(0, headBone.position.y * 0.04f, 1 * enabledAvatarDescriptor.transform.localScale.z * headBone.localScale.z);
            previewCamera.transform.position = headPosition;

            var gameView = GetGameView();
            GetTargetDisplay(gameView).SetValue(GetEditorWindow(gameView), previewCamera.targetDisplay);
        }

        public static void CreatePreviewCamera()
        {
            previewCamera = GameObject.FindObjectsOfType<Camera>().FirstOrDefault(c => c.name == "PreviewCamera");
            if (previewCamera != null)
            {
                return;
            }

            var gameObject = new GameObject("PreviewCamera", typeof(Camera));
            gameObject.transform.position = new Vector3(0, 1, 1);
            gameObject.transform.rotation = Quaternion.Euler(0, 180, 0);
            SceneManager.MoveGameObjectToScene(gameObject, scene);

            previewCamera = gameObject.GetComponent<Camera>();
            previewCamera.fieldOfView = 10;
            previewCamera.nearClipPlane = .3f;
            previewCamera.farClipPlane = 1000;
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = Color.gray;
            previewCamera.cameraType = CameraType.Preview;
            previewCamera.forceIntoRenderTexture = true;
            previewCamera.scene = scene;
            previewCamera.enabled = true;
            previewCamera.transform.SetSiblingIndex(0);

            var gameView = GetGameView();
            var currentTargetDisp = GetTargetDisplay(gameView).GetValue(GetEditorWindow(gameView));
            previewCamera.targetDisplay = (int)currentTargetDisp + 1;
        }
    }
}