using System.Collections;
using TMPro;
using UnityEngine;

public class SuccessTextTyping : MonoBehaviour
{
    [Header("Text Variables")]
    [SerializeField] private TextMeshPro textMeshPro;
    [SerializeField] private float typingSpeed = 0.05f; // ���� �� Ÿ���� �ӵ�
    [SerializeField] private float waitTime = 3f; // ���� �Ϸ� ��, ��� �ð�

    [Header("Crown GameObject")]
    [SerializeField] private GameObject crown; // �հ�

    // ǥ���� ���� ����Ʈ
    private string[] successTexts =
    {
        "Congratulations! \n You are the winner!",
        "You became \n the Best Horse!",
        "Take your Crown!!"
    };

    private int currentSentenceIndex = 0; // ���� ���� �ε���
    private bool isTyping = false; // Ÿ���� ������ üũ



    private void Start()
    {
        if (textMeshPro == null)
        {
            textMeshPro = GetComponent<TextMeshPro>();
        }

        StartTyping();
    }

    private void StartTyping()
    {
        if (currentSentenceIndex < successTexts.Length)
        {
            StartCoroutine(TypeSentence(successTexts[currentSentenceIndex]));
        }
    }

    // ������ �ѱ��� �� ġ�� �ڷ�ƾ
    IEnumerator TypeSentence(string _sentence)
    {
        isTyping = true;
        textMeshPro.text = ""; // �ؽ�Ʈ �ʱ�ȭ

        // �ѱ��ھ� �߰�
        foreach (char letter in _sentence)
        {
            textMeshPro.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;

        // ���� �Ϸ� �� ���
        yield return new WaitForSeconds(waitTime);

        // ������ ������ �ƴҶ��� �ؽ�Ʈ �����
        // ������ �����̸� �հ� Ȱ��ȭ
        if (currentSentenceIndex < (successTexts.Length - 1))
        {
            yield return StartCoroutine(ClearText());
        }
        else
        {
            crown.SetActive(true);
        }

            // ���� �������� �Ѿ��
            currentSentenceIndex++;
        if (currentSentenceIndex < successTexts.Length)
        {
            StartCoroutine(TypeSentence(successTexts[currentSentenceIndex]));
        }
        else
        {
            // ��� ������ ������ ���� �ƹ��ϵ� ����
        }
    }

    IEnumerator ClearText()
    {
        // �� ���ھ� �����
        while (textMeshPro.text.Length > 0)
        {
            textMeshPro.text = textMeshPro.text.Substring(0, textMeshPro.text.Length - 1);
            yield return new WaitForSeconds(typingSpeed / 3);
        }
    }


}
