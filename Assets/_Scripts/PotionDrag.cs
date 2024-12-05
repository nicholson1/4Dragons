using System.Collections;
using System.Collections.Generic;
//using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PotionDrag : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
{
       [SerializeField] private ToolTip _toolTip;
       public Consumable potion;
       [SerializeField] private Image image;
       private RectTransform _rectTransform;
       private Canvas canvas;
       private Transform holder;
       private Vector3 startPos;
       
       
       [SerializeField] private AudioClip usePotion;
       [SerializeField] private float usePotionVol;

       public void InitializePotion(Consumable p)
       {
              if (_rectTransform == null)
              {
                     _rectTransform = GetComponent<RectTransform>();
                     canvas = transform.parent.parent.GetComponent<Canvas>();
                     holder = transform.parent;
              }
              potion = p;
              _toolTip.e = potion;
              image.sprite = p.icon;
              
              TutorialManager.Instance.QueueTip(TutorialNames.Potions);
       }

       public void OnBeginDrag(PointerEventData eventData)
       {
              //throw new System.NotImplementedException();
              //tartPos
              startPos = _rectTransform.anchoredPosition;
              holder.GetComponent<CanvasGroup>().blocksRaycasts = false;

              //_rectTransform.SetParent(transform.parent);
              
       }
       

       public void OnDrag(PointerEventData eventData)
       {
              //throw new System.NotImplementedException();
              _rectTransform.anchoredPosition += eventData.delta/ canvas.scaleFactor;
       }

       public void OnDrop(PointerEventData eventData)
       {

              

       }

       public void OnEndDrag(PointerEventData eventData)
       {
              int layer_mask = LayerMask.GetMask("Characters");
              Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
              RaycastHit hit;

              // Check if the ray hits any object
              if (Physics.Raycast(ray, out hit,100,layer_mask))
              {
                     CombatEntity CE = hit.transform.gameObject.GetComponent<CombatEntity>();
                     if (CE != null)
                     {
                            CE.HitWithPotion(potion.ConsumableType);
                            EquipmentManager._instance.PoolPotion(this);
                            
                            SoundManager.Instance.Play2DSFX(usePotion, usePotionVol, 1, .05f);
                            
                     }
              }

              holder.GetComponent<CanvasGroup>().blocksRaycasts = true;

              //else reset to the pos start position
              _rectTransform.anchoredPosition = startPos;
       }

       public void OnPointerDown(PointerEventData eventData)
       {
              //throw new System.NotImplementedException();
       }
}
