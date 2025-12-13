using UnityEngine;

public class MobileUIHandler : MonoBehaviour
{
    [Header("Kéo GameObject 'MobileControls' vào đây")]
    public GameObject mobileControls;

    void Start()
    {
        // Logic kiểm tra nền tảng

        #if UNITY_ANDROID || UNITY_IOS
            // Nếu là Android hoặc iPhone -> BẬT nút
            mobileControls.SetActive(true);
        #else
            // Nếu là PC (Windows/Mac) hoặc Web -> TẮT nút
            mobileControls.SetActive(false);
        #endif

        // --- MẸO DÀNH CHO BẠN ---
        // Nếu bạn muốn Test nút ảo ngay trong Unity Editor (trên máy tính)
        // thì bỏ dấu comment (//) ở dòng dưới đi nhé:
        
        // if (Application.isEditor) mobileControls.SetActive(true);
    }
}