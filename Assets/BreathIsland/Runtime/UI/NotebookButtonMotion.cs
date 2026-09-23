using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace BreathIsland
{
    public sealed class NotebookButtonMotion : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerDownHandler,IPointerUpHandler
    {
        private float target=1;
        public void OnPointerEnter(PointerEventData data){if(GetComponent<Button>().interactable)target=1.025f;}
        public void OnPointerExit(PointerEventData data)=>target=1;
        public void OnPointerDown(PointerEventData data){if(GetComponent<Button>().interactable)target=.965f;}
        public void OnPointerUp(PointerEventData data)=>target=1;
        private void OnDisable(){target=1;transform.localScale=Vector3.one;}
        private void LateUpdate()=>transform.localScale=Vector3.Lerp(transform.localScale,Vector3.one*target,1-Mathf.Exp(-22*Time.unscaledDeltaTime));
    }
}
