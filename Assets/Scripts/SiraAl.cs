
using System.Collections.Generic;
using Mono.Data.Sqlite;
using System;
using System.Data;
using UnityEngine;
using UnityEngine.UI;


public class SiraAl : MonoBehaviour {
  private string  sqlQuery,connection;

    IDbConnection dbconn;
    // Use this for initialization

    public InputField tcno;
    public Text fis;
    public Text V1;
    public Text V2;
    public Text V3;
    Queue<Musteri> islem_sirasi = new Queue<Musteri>();
    Queue<Musteri> banka_musteri_sirasi = new Queue<Musteri>();
    Queue<Musteri> normal_musteri_sirasi = new Queue<Musteri>();
    List<Vezne> vezneler = new List<Vezne>();
    int normal_sayac = 0,banka_musterisi_sayac=1000,oncelik_derecesi=2;
    public void OpenDB()
    {
        string DbName = "banka.db3";
       
      
        string filepath = Application.dataPath + "/Plugins/" + DbName;
        //veritabanı baglantı
        connection = "URI=file:" + filepath;
       
       

    }

    private void Start()
    {   
        OpenDB();  //veritabanı  baglantı kur

        //acılısta vezneleri olustur
        Vezne vezne1 = new Vezne();
        vezne1.veznetext = V1;
        vezne1.bosmu = true;
        vezneler.Add(vezne1);
        Vezne vezne2 = new Vezne();
        vezne2.veznetext = V2;
        vezne2.bosmu = true;
        vezneler.Add(vezne2);
        Vezne vezne3 = new Vezne();
        vezne3.bosmu = true;
        vezne3.veznetext = V3;
        vezneler.Add(vezne3);
        V1.text = "0000";
        V2.text = "0000";
        V3.text = "0000";
    }
    public void SiraAlButon()
    {   // Sira al butonu eventi musteri olusur veritabanından kontrol edilerek banka musterisi mi değilmi
        //ona göre bankamusteri kuyruguna ya da normal musteri kuyruguna atılır 
        Musteri musteri = new Musteri();
        musteri.tcno = tcno.text;
        musteri.sira_zaman = DateTime.Now;
        //karsılastırma veritabanından tc banka musterisiyse o kuyruga at
        string str = tcno.text;
        if (readers(str))
        {
            banka_musteri_sirasi.Enqueue(musteri);
            banka_musterisi_sayac++;
            musteri.sira = banka_musterisi_sayac;
        }
        else
        {
            musteri.oncelik_sayac = 0;
            normal_musteri_sirasi.Enqueue(musteri);
            normal_sayac++;
            musteri.sira = normal_sayac;
        }
     
        //ekranda sıra numarası gsteren yazı oluşturulur.
        fis.text = "Sıra numaranız\n" + musteri.sira;
      
    }
    public bool readers(string tc)
    {   //tc no veritabanında varsa true yoksa false döndüren method
        using (dbconn = new SqliteConnection(connection))
        {
            dbconn.Open(); //veritabanı con aç
            IDbCommand dbcmd = dbconn.CreateCommand();
           
            sqlQuery = "SELECT * " + "FROM Musteriler";
            dbcmd.CommandText = sqlQuery;
            IDataReader reader = dbcmd.ExecuteReader();
            while (reader.Read())
            {
                //int id = reader.GetInt32(0);
                string tcno = reader.GetString(1);
                

                
                 if (tc==tcno)
                  {
                     reader.Close();
                      reader = null;
                    dbcmd.Dispose();
                     dbcmd = null;
                     dbconn.Close();
                     dbconn = null;
                    return true;
                 }
            }
            reader.Close();
            reader = null;
            dbcmd.Dispose();
            dbcmd = null;
            dbconn.Close();
            dbconn = null;
            return false;
        }
    }
    public void vezneTakip()
    { //veznenin boş mu kontrolunu yapar boş ise islemeAl() methodunu çalıtırır.
      //vezneler 30-60 saniye arasında rasgele sürelerde işlem yapar.

        foreach (var item in vezneler)
        {
            if (item.bosmu)
            {
                islemeAl();
                if (islem_sirasi.Count != 0)
                {
                    Musteri siradaki_musteri = (Musteri)islem_sirasi.Dequeue();
                    item.veznetext.text =Convert.ToString(siradaki_musteri.sira);
                    item.bosmu = false;
                    item.lefttime = DateTime.Now;
                }
               
            }
            
            TimeSpan fark = DateTime.Now-item.lefttime;
            
            if (fark.Seconds>UnityEngine.Random.Range(30,60))
            {
                item.bosmu = true;
            }
        }
    }
    public void islemeAl()
    {
        //vezne boş ise bankamusterisi ve normal musteri kuyruklarının ilk elemanları karşılaştırlır.
        //hangisi öncelikli ise islemkuyruguna alınır
        //kendinden sonra gelen banka musterisi normal musteriyi geçer 2 öncelikli müsteriden sonra normal musteri önüne 
        //geçilmez.

        if (banka_musteri_sirasi.Count!=0 && normal_musteri_sirasi.Count!=0)
        {
            Musteri bmusteri = (Musteri)banka_musteri_sirasi.Peek();
            Musteri nmusteri = (Musteri)normal_musteri_sirasi.Peek();
            if (bmusteri.sira_zaman< nmusteri.sira_zaman)
            {   //banka musterisi önce gelmişse sıra zaten onundur.
                islem_sirasi.Enqueue(banka_musteri_sirasi.Dequeue());
                return; 
            }



            if (nmusteri.oncelik_sayac==oncelik_derecesi)
            {   //2 kere önüne geçilen normal musterinin önüne artık geçilmez(6 normal musteriye kadar) normal musteri işleme girer
                islem_sirasi.Enqueue(normal_musteri_sirasi.Dequeue());
               
            }
            else
            {   //kendinden sonra gelen banka musterisi öncelik sayesinde normal musteriden önce isleme girer
                islem_sirasi.Enqueue(banka_musteri_sirasi.Dequeue());
                int i = 0;
                foreach (var item in normal_musteri_sirasi)
                {
                    i++;
                    if (item.sira_zaman<bmusteri.sira_zaman && i<6)
                    {
                        item.oncelik_sayac++;
                    }
                    else
                    {
                        return;
                    }
                }
              
            }

        }
        else if (normal_musteri_sirasi.Count == 0 && banka_musteri_sirasi.Count == 0)
        {   //bankada hiç kimse yoktur
            return;
        }
        else if (normal_musteri_sirasi.Count == 0)
        {   //normal musteri yoksa banka musterisi isleme girer
            islem_sirasi.Enqueue(banka_musteri_sirasi.Dequeue());
        }
        else if (banka_musteri_sirasi.Count==0)
        {   //banka musterisi yoksa normal musteri isleme girer
            islem_sirasi.Enqueue(normal_musteri_sirasi.Dequeue());  
        }
    }
    // Update is called once per frame
    private void Update()
    {   //her frame için çalışan update methodu 
        vezneTakip();      
    }
}

public class Musteri
{
    public string tcno;
    public int sira;
    public DateTime sira_zaman;
    public int oncelik_sayac;

}

public class Vezne
{
    public Text veznetext;
  
    public bool bosmu;
    public DateTime lefttime;
}
