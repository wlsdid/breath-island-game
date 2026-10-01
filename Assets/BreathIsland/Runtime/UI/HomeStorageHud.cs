using UnityEngine;
namespace BreathIsland
{
    public sealed class HomeStorageHud : HudScreen
    {
        private readonly HomeStorageSystem storage;
        private int selected,bagPage,quantity=1;
        private bool fromChest;
        private string opened;
        public HomeStorageHud(GameContext context,GamePhaseController phases,HomeStorageSystem storage):base(context,phases){this.storage=storage;}
        public override void Draw(float width,float height)
        {
            if(opened!=storage.OpenId){opened=storage.OpenId;selected=0;bagPage=0;quantity=1;fromChest=false;}
            float x=(width-1340)/2,y=(height-780)/2;
            ui.Panel(new Rect(x,y,1340,780));
            ui.Icon(new Rect(x+30,y+24,60,60),"chest");
            ui.Label(new Rect(x+110,y+15,750,70),context.Text("home_storage_title"),titleStyle);
            ui.Button(new Rect(x+1140,y+27,160,48),context.Text("bag_close"),()=>phases.Set(GamePhase.Home));
            ui.Label(new Rect(x+35,y+91,610,43),context.Text("bag_title"),bodyStyle);
            ui.Label(new Rect(x+700,y+91,605,43),context.Text("home_storage_capacity"),bodyStyle);
            var bag=storage.BagItems();int pages=Mathf.Max(1,Mathf.CeilToInt(bag.Count/8f));bagPage=Mathf.Clamp(bagPage,0,pages-1);
            for(int i=bagPage*8;i<Mathf.Min(bag.Count,(bagPage+1)*8);i++)
            {
                int index=i;var item=bag[i];float bx=x+35+(i%4)*153,by=y+149+((i%8)/4)*160;
                ui.Button(new Rect(bx,by,143,156),"",()=>{selected=index;fromChest=false;quantity=1;},!fromChest&&selected==i?NotebookStyle.Selected:buttonStyle);
                ui.Icon(new Rect(bx+44,by+8,55,55),item.Id);
                ui.Label(new Rect(bx+5,by+62,133,56),context.Text(item.NameKey),!fromChest&&selected==i?NotebookStyle.Light:NotebookStyle.Center);
                ui.Label(new Rect(bx+5,by+120,133,32),item.Count.ToString(),!fromChest&&selected==i?NotebookStyle.Light:NotebookStyle.Center);
            }
            ui.Button(new Rect(x+35,y+475,285,42),context.Text("bag_previous"),()=>bagPage--,buttonStyle,bagPage>0);
            ui.Button(new Rect(x+355,y+475,285,42),context.Text("bag_next"),()=>bagPage++,buttonStyle,bagPage+1<pages);
            var chest=storage.Current;
            if(chest==null)return;
            for(int i=0;i<16;i++)
            {
                int index=i;var slot=chest.slots[i];float bx=x+700+(i%4)*153,by=y+149+(i/4)*112;
                ui.Button(new Rect(bx,by,143,103),"",()=>{selected=index;fromChest=true;quantity=1;},fromChest&&selected==i?NotebookStyle.Selected:buttonStyle);
                if(slot.count<=0)continue;
                ui.Icon(new Rect(bx+7,by+8,40,40),slot.id);
                ui.Label(new Rect(bx+65,by+7,68,40),slot.count.ToString(),fromChest&&selected==i?NotebookStyle.Light:NotebookStyle.Center);
                ui.Label(new Rect(bx+5,by+47,133,54),context.Text(storage.NameKey(slot)),fromChest&&selected==i?NotebookStyle.Light:NotebookStyle.Center);
            }
            int available=fromChest?chest.slots[Mathf.Clamp(selected,0,15)].count:bag[Mathf.Clamp(selected,0,bag.Count-1)].Count;
            quantity=Mathf.Clamp(quantity,1,Mathf.Max(1,available));
            ui.Label(new Rect(x+35,y+537,605,65),context.Text("home_storage_help"),smallStyle);
            ui.Button(new Rect(x+35,y+619,70,45),"−",()=>quantity=Mathf.Max(1,quantity-1),compact:true);
            ui.Label(new Rect(x+113,y+619,170,45),string.Format(context.Text("home_quantity"),quantity),centeredStyle);
            ui.Button(new Rect(x+290,y+619,70,45),"+",()=>quantity=Mathf.Min(available,quantity+1),compact:true);
            ui.Button(new Rect(x+375,y+619,110,45),"−10",()=>quantity=Mathf.Max(1,quantity-10),compact:true);
            ui.Button(new Rect(x+500,y+619,140,45),"+10",()=>quantity=Mathf.Min(available,quantity+10),compact:true);
            ui.Label(new Rect(x+700,y+614,605,45),context.Text(fromChest?"home_withdraw":"home_deposit"),bodyStyle);
            ui.Button(new Rect(x+35,y+694,405,54),context.Text("home_transfer_one"),()=>Transfer(1),buttonStyle,available>0);
            ui.Button(new Rect(x+465,y+694,405,54),context.Text("home_transfer_quantity"),()=>Transfer(quantity),primaryButtonStyle,available>0);
            ui.Button(new Rect(x+895,y+694,405,54),context.Text("home_transfer_all"),()=>Transfer(int.MaxValue),buttonStyle,available>0);
        }
        private void Transfer(int count)
        {
            int moved;
            if(fromChest)moved=storage.Withdraw(selected,count);
            else {var item=storage.BagItems()[selected];moved=storage.Deposit(item.Id,selected>=6,count);}
            if(moved==0)context.Message(context.Text("home_storage_full"));
        }
    }
}
