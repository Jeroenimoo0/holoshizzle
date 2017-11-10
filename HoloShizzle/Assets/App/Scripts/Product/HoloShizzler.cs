using HoloToolkitExtensions.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class HoloShizzler : MonoBehaviour {

    [System.Serializable]
    public class ItemModel
    {
        public string itemId;
        public GameObject model;
    }

    public ItemModel[] models;
    public GameObject spawnPosition;

    [SerializeField]
    private GameObject productPrefab;

    private List<Product> _products = new List<Product>();

	private IEnumerator Start ()
    {
        Messenger.Instance.AddListener<ResetMessage>(DoReset);

        var headers = new Dictionary<string, string>();
        headers.Add("Cookie", "rxVisitor=1491476043930BF7KE70FTURH128KU26Q59CMGT2R7HEN; cookies-accepted=1.5; SSOD=ANwWAAAAAAA4ADYAhiAAAAFodaGOAUb7484BdgtaZwG63flKAapj0SUB9bsf3AG33-ZYA2vf49YBq5qo6wFbMMKq; __gads=ID=373dd39870c756d1:T=1504017357:S=ALNI_Mboa9A8m3VyLD_TNFqovVOZWBQ4-w; ui_scaling=size_s%20size_m%20size_l%20size_xl%20size_xxl; __cfduid=d2514e3b3ad9f345a90544f817f2996331504621787; rxVisitor=1495444244141VO7K85K2MVLI780O35K3UHT568DS99PB; dtPC=-; dtLatC=1; dtSa=-; dtCookie=1$CF0B2AC4E2DB795198E234556D025397|_default|1|RUM+Default+Application|1; SSLB=0; SSID=CAA3cB0AACAAAABFHuZYbZ9BAEUe5lghAAAAAAAAAAAAZmcFWgBUmgIBAAAUAQAAbAEAAEwBAAB0AQAASgEAAIQBAAAvAQAA; SSSC=4.G6405840801686986605.33|0.0; SSRT=ZmcFWgADAQ; SSPV=AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA; TS0163d06f=01b71d1f1cc6927361be7633be0fb68bd2259f146d4ad8e47a093caa765416e14e79bfd2b017555ad6351517f84b1d5f4c1523a2c24878c7a4d73502e727c874b86d9cee7c0da7db39ac55720e61767d10039098e51d434fdac66f6e1de6182c68d4c4c412a4d6eeacb8b30dc5f0c81ae56340542d; gaClientId=805377975.1504017338; JSESSIONID=EBB51B51B543278351D4D06968845EDD.ahws_4; TS01fb4f52_77=5292_29fde4b20c1f7230_rsb_0_rs_%2Fmijn%2Fuitloggen_rs_1_rs_0; ahold_presumed_member_no=55439162_CHK2wzC0ZuEgawjNLVZd7Ll2w%3D%3D; ahold_sso_status=55439162.1.1.20171110103613000.59AAD58BE1AFBC431F5448899FA2FA3F-ahws-1.20171110103613581.629204_CHKDT55Avw%2BF1T1OfPaFWwkOw%3D%3D; _gat_mainTracker=1; _gat_initialTracker=1; dtSa=-; _ga=GA1.2.805377975.1504017338; _gid=GA1.2.2128296364.1510150943; dtPC=107898270_620h1; dtCookie=F9DA03182381A61ADB1D7F29B987CFA9|d3d3LmFoLm5sfDE; TS01fb4f52=01b71d1f1c48d75530ad9789c61b9bec1b1efaf6124d7cfee90fb63a10c5cfa363e20cedf3987c4e07ba35d1ba31805077e0ecaeea6aa119552c82d96137dd4d6075a5352675aac3d13327b1c782bd9c09ff635ba7f238959aeab1f8271baf4f70129c14540c2fa07c9e30fc2fe165eef7354b77d8c06060d1ab0536f79eabb820cef91299221265e94873fa6e50c6b8b0ff95baac; TS0172c9cf=01b71d1f1c10edbbaf31eb77cc88b17fce161a5ab9b288d477994ab542f6e1b765be9f71f9a44772b59007dd1c23c2391c3b445ee0; dtLatC=5");

        while (true)
        {
            var request = new WWW("https://www.ah.nl/service/rest/delegate?url=%2Fmijnlijst&_=1510307898401", null, headers);

            yield return request;
            Debug.Log(request.text);

            var root = JsonConvert.DeserializeObject<GeneratedItemList.RootObject>(request.text);

            foreach(var lane in root._embedded.lanes)
            {
                if(lane.type == "ShoppingListLane")
                {
                    yield return UpdateProductList(lane._embedded.items);
                }
            }

            yield return new WaitForSeconds(1.0f);
        }
	}

    private void DoReset(ResetMessage message)
    {
        foreach(var product in _products)
        {
            Destroy(product.gameObject);
        }

        _products.Clear();
    }

    private IEnumerator UpdateProductList(List<GeneratedItemList.Item> items)
    {
        // Add loop
        foreach (var item in items)
        {
            var id = item._embedded.product.id;

            bool found = false;
            foreach (var existingProduct in _products)
            {
                if (existingProduct.id == id)
                {
                    found = true;
                }
            }
            if (found) continue;

            Vector3 spawnPos;

            if(spawnPosition != null)
            {
                spawnPos = spawnPosition.transform.position;
            }
            else
            {
                spawnPos = Vector3.zero;
            }

            var gameObject = Instantiate(productPrefab, spawnPos, Quaternion.identity);
            var product = gameObject.GetComponent<Product>();

            var model = Array.Find(models, (modelItem) => modelItem.itemId == id);

            if(model != null)
            {
                Instantiate(model.model, gameObject.transform);
            }

            var www = new WWW("https://www.ah.nl/service/rest/delegate?url=" + item.navItem.link.href);

            yield return www;

            var test = JsonConvert.DeserializeObject<GeneratedItemInfo.RootObject>(www.text);

            var infosList = new List<string>();

            foreach (var lane in test._embedded.lanes)
            {
                if(lane.type == "StoryLane")
                {
                    foreach(var item2 in lane._embedded.items)
                    {
                        if(item2._embedded.sections[0]._embedded.content[0].text != null && item2._embedded.sections[0]._embedded.content[0].text.title == "Voedingswaarden")
                        {
                            var firstRegex = new Regex(@"\[[\[\]a-z\/]{1,}\]([\.A-Z a-z0-9]{1,})");
                            var regex = new Regex(@"\[[\[\]a-z\/]{1,}\]([\.A-Z a-z0-9]{1,})[\[\]a-z\/]{1,}([\.A-Z a-z0-9()]{1,})");

                            var body = item2._embedded.sections[0]._embedded.content[2].text.body;

                            var perWhat = firstRegex.Match(body).Groups[1];
                            infosList.Add(perWhat.ToString());

                            body = firstRegex.Replace(body, "", 1);

                            var matches = regex.Matches(body);

                            foreach(Match match in matches)
                            {
                                infosList.Add(match.Groups[1] + ": " + match.Groups[2]);
                            }

                            goto exitLoop;
                        }
                    }
                }
            }
            exitLoop:

            var imgTest = new WWW(item._embedded.product.images[0].link.href);

            yield return imgTest;

            product.description = item._embedded.product.description;
            product.id = id;
            product.Infos = infosList.ToArray();
            product.GetComponentInChildren<MeshRenderer>().material.SetTexture("_MainTex", imgTest.texture);

            Debug.Log("Adding " + product.description);

            _products.Add(product);
        }

        // Remove loop
        foreach (var existingProduct in new List<Product>(_products))
        {
            bool found = false;
            foreach (var item in items)
            {
                if(item._embedded.product.id == existingProduct.id)
                {
                    found = true;
                    break;
                }
            }


            if(!found)
            {
                Debug.Log("Removing " + existingProduct.description);

                _products.Remove(existingProduct);
                Destroy(existingProduct.gameObject);
            }
        }
    }

    public class GeneratedItemInfo
    {
        public class Item
        {
            public string name { get; set; }
            public string content { get; set; }
        }

        public class MetaTags
        {
            public List<Item> items { get; set; }
            public string label { get; set; }
        }

        public class Parameters
        {
            public string ah_order_mode { get; set; }
            public string ah_regtype { get; set; }
            public string ah_site { get; set; }
            public string ah_state { get; set; }
            public string name { get; set; }
            public string ns__t { get; set; }
            public string ns_c { get; set; }
            public string ns_jspageurl { get; set; }
            public string ns_ti { get; set; }
            public string ns_referrer { get; set; }
        }

        public class Analytics
        {
            public string baseUrl { get; set; }
            public Parameters parameters { get; set; }
            public string label { get; set; }
        }

        public class Page
        {
            public string type { get; set; }
            public string taxonomyNodes { get; set; }
            public string label { get; set; }
        }

        public class Meta
        {
            public MetaTags metaTags { get; set; }
            public Analytics analytics { get; set; }
            public Page page { get; set; }
        }

        public class Self
        {
            public string href { get; set; }
        }

        public class Links
        {
            public Self self { get; set; }
        }

        public class ColorScheme
        {
            public string baseColor { get; set; }
            public bool inverted { get; set; }
        }

        public class Link
        {
            public string href { get; set; }
        }

        public class Image
        {
            public string title { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public Link link { get; set; }
        }

        public class Link2
        {
            public string href { get; set; }
            public string pageType { get; set; }
        }

        public class NavItem
        {
            public Link2 link { get; set; }
            public string title { get; set; }
        }

        public class Tag
        {
            public string text { get; set; }
        }

        public class Link3
        {
            public string href { get; set; }
        }

        public class Image2
        {
            public int width { get; set; }
            public int height { get; set; }
            public Link3 link { get; set; }
        }

        public class Link4
        {
            public string href { get; set; }
            public string pageType { get; set; }
            public string target { get; set; }
        }

        public class NavItem2
        {
            public Link4 link { get; set; }
        }

        public class Shop
        {
            public string title { get; set; }
            public string description { get; set; }
            public Tag tag { get; set; }
            public Image2 image { get; set; }
            public NavItem2 navItem { get; set; }
        }

        public class Autosuggest
        {
            public string href { get; set; }
        }

        public class Search
        {
            public string href { get; set; }
            public string pageType { get; set; }
        }

        public class SeoBreadcrumb
        {
            public string title { get; set; }
            public string url { get; set; }
        }

        public class Add
        {
            public string href { get; set; }
            public string method { get; set; }
        }

        public class Links2
        {
            public Add add { get; set; }
        }

        public class Analytics2
        {
            public string originCode { get; set; }
        }

        public class Availability
        {
            public bool orderable { get; set; }
            public string label { get; set; }
        }

        public class PriceLabel
        {
            public double now { get; set; }
        }

        public class Link5
        {
            public string href { get; set; }
        }

        public class Image3
        {
            public string title { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public Link5 link { get; set; }
        }

        public class Details
        {
            public string unitOfUseSize { get; set; }
            public string summary { get; set; }
            public string seoDescription { get; set; }
        }

        public class Product
        {
            public string id { get; set; }
            public string description { get; set; }
            public string unitSize { get; set; }
            public string brandName { get; set; }
            public string categoryName { get; set; }
            public Availability availability { get; set; }
            public PriceLabel priceLabel { get; set; }
            public List<Image3> images { get; set; }
            public Details details { get; set; }
        }

        public class Link6
        {
            public string href { get; set; }
            public string pageType { get; set; }
        }

        public class NavItem3
        {
            public Link6 link { get; set; }
        }

        public class Add2
        {
            public string href { get; set; }
            public string method { get; set; }
        }

        public class Links3
        {
            public Add2 add { get; set; }
        }

        public class Analytics3
        {
            public string originCode { get; set; }
            public string crossSellParentId { get; set; }
        }

        public class Availability2
        {
            public bool orderable { get; set; }
            public string label { get; set; }
        }

        public class PriceLabel2
        {
            public double now { get; set; }
        }

        public class Link7
        {
            public string href { get; set; }
        }

        public class Image4
        {
            public string title { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public Link7 link { get; set; }
        }

        public class Product2
        {
            public string id { get; set; }
            public string description { get; set; }
            public string unitSize { get; set; }
            public string brandName { get; set; }
            public string categoryName { get; set; }
            public Availability2 availability { get; set; }
            public PriceLabel2 priceLabel { get; set; }
            public List<Image4> images { get; set; }
        }

        public class Embedded4
        {
            public Analytics3 analytics { get; set; }
            public Product2 product { get; set; }
        }

        public class Item3
        {
            public NavItem3 navItem { get; set; }
            public string foldOutChannelId { get; set; }
            public string resourceType { get; set; }
            public string type { get; set; }
            public Links3 _links { get; set; }
            public string context { get; set; }
            public string viewType { get; set; }
            public Embedded4 _embedded { get; set; }
        }

        public class Link8
        {
            public string href { get; set; }
            public string pageType { get; set; }
        }

        public class NavItem4
        {
            public string title { get; set; }
            public Link8 link { get; set; }
        }

        public class Embedded5
        {
            public object filterItems { get; set; }
        }

        public class Filter
        {
            public string label { get; set; }
            public NavItem4 navItem { get; set; }
            public bool sorted { get; set; }
            public Embedded5 _embedded { get; set; }
        }

        public class Text
        {
            public string title { get; set; }
            public string body { get; set; }
            public string subtitle { get; set; }
        }

        public class Content
        {
            public Text text { get; set; }
            public string resourceType { get; set; }
            public string type { get; set; }
        }

        public class Embedded6
        {
            public List<Content> content { get; set; }
        }

        public class Section
        {
            public string resourceType { get; set; }
            public string type { get; set; }
            public Embedded6 _embedded { get; set; }
        }

        public class Embedded3
        {
            public Analytics2 analytics { get; set; }
            public Product product { get; set; }
            public List<Item3> items { get; set; }
            public List<Filter> filters { get; set; }
            public List<object> breadcrumbs { get; set; }
            public List<Section> sections { get; set; }
        }

        public class Item2
        {
            public string id { get; set; }
            public Image image { get; set; }
            public NavItem navItem { get; set; }
            public string title { get; set; }
            public string resourceType { get; set; }
            public string type { get; set; }
            public List<Shop> shops { get; set; }
            public Autosuggest autosuggest { get; set; }
            public Search search { get; set; }
            public string searchType { get; set; }
            public List<string> frequentProductSearchTerms { get; set; }
            public string color { get; set; }
            public string backgroundColor { get; set; }
            public List<SeoBreadcrumb> seoBreadcrumb { get; set; }
            public string foldOutChannelId { get; set; }
            public Links2 _links { get; set; }
            public string context { get; set; }
            public string viewType { get; set; }
            public Embedded3 _embedded { get; set; }
            public bool? expanded { get; set; }
            public int? value { get; set; }
            public string body { get; set; }
        }

        public class Embedded2
        {
            public List<Item2> items { get; set; }
        }

        public class Lane
        {
            public string id { get; set; }
            public bool gutter { get; set; }
            public bool expanded { get; set; }
            public ColorScheme colorScheme { get; set; }
            public string type { get; set; }
            public Embedded2 _embedded { get; set; }
            public string layout { get; set; }
            public string backgroundColor { get; set; }
        }

        public class Embedded
        {
            public List<Lane> lanes { get; set; }
        }

        public class RootObject
        {
            public string title { get; set; }
            public Meta _meta { get; set; }
            public Links _links { get; set; }
            public Embedded _embedded { get; set; }
        }
    }

    public class GeneratedItemList
    {
        public class ShoppingContext2
        {
            public bool isOnlineOrder { get; set; }
            public bool isObtainmentChangeable { get; set; }
            public string shoppingType { get; set; }
            public object orderState { get; set; }
            public string userName { get; set; }
        }

        public class ShoppingContext
        {
            public ShoppingContext2 shoppingContext { get; set; }
            public string label { get; set; }
        }

        public class ShoppingList
        {
            public int itemCount { get; set; }
            public int itemsInOrderCount { get; set; }
            public float totalPrice { get; set; }
            public int totalBonusDiscount { get; set; }
            public string label { get; set; }
        }

        public class Parameters
        {
            public string ah_order_mode { get; set; }
            public string ah_regtype { get; set; }
            public string ah_site { get; set; }
            public string ah_state { get; set; }
            public string name { get; set; }
            public string ns__t { get; set; }
            public string ns_c { get; set; }
            public string ns_jspageurl { get; set; }
            public string ns_referrer { get; set; }
        }

        public class Analytics
        {
            public string baseUrl { get; set; }
            public Parameters parameters { get; set; }
            public string label { get; set; }
        }

        public class Page
        {
            public string type { get; set; }
            public string label { get; set; }
        }

        public class DataLayerUser
        {
            public string label { get; set; }
            public string ID { get; set; }
            public string CT { get; set; }
            public string ST { get; set; }
            public List<string> OP { get; set; }
            public List<string> CA { get; set; }
        }

        public class Pixels
        {
            public List<string> tags { get; set; }
            public string label { get; set; }
        }

        public class Meta
        {
            public ShoppingContext shoppingContext { get; set; }
            public ShoppingList shoppingList { get; set; }
            public List<object> notifications { get; set; }
            public Analytics analytics { get; set; }
            public Page page { get; set; }
            public DataLayerUser dataLayerUser { get; set; }
            public Pixels pixels { get; set; }
        }

        public class Link
        {
            public string href { get; set; }
        }

        public class NavItem
        {
            public string title { get; set; }
            public Link link { get; set; }
        }

        public class Text
        {
            public string title { get; set; }
            public string body { get; set; }
        }

        public class Link2
        {
            public string href { get; set; }
            public string pageType { get; set; }
        }

        public class NavItem2
        {
            public Link2 link { get; set; }
        }

        public class Add
        {
            public string href { get; set; }
            public string method { get; set; }
        }

        public class Links
        {
            public Add add { get; set; }
        }

        public class Analytics2
        {
            public string originCode { get; set; }
        }

        public class Update
        {
            public string href { get; set; }
            public string method { get; set; }
            public string mediaType { get; set; }
        }

        public class Delete
        {
            public string href { get; set; }
            public string method { get; set; }
        }

        public class Links2
        {
            public Update update { get; set; }
            public Delete delete { get; set; }
        }

        public class ListItem
        {
            public int quantity { get; set; }
            public string id { get; set; }
            public Links2 _links { get; set; }
        }

        public class Availability
        {
            public bool orderable { get; set; }
            public string label { get; set; }
        }

        public class PriceLabel
        {
            public double now { get; set; }
        }

        public class Link3
        {
            public string href { get; set; }
        }

        public class Image
        {
            public string title { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public Link3 link { get; set; }
        }

        public class Sticker
        {
            public string label { get; set; }
            public string detailText { get; set; }
            public Image image { get; set; }
        }

        public class Link4
        {
            public string href { get; set; }
        }

        public class Image2
        {
            public string title { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public Link4 link { get; set; }
        }

        public class Product
        {
            public string id { get; set; }
            public string description { get; set; }
            public string unitSize { get; set; }
            public string brandName { get; set; }
            public string categoryName { get; set; }
            public Availability availability { get; set; }
            public PriceLabel priceLabel { get; set; }
            public Sticker sticker { get; set; }
            public List<Image2> images { get; set; }
            public List<string> propertyIcons { get; set; }
        }

        public class Embedded3
        {
            public Analytics2 analytics { get; set; }
            public ListItem listItem { get; set; }
            public Product product { get; set; }
        }

        public class Link5
        {
            public string href { get; set; }
            public string pageType { get; set; }
        }

        public class Item
        {
            public string resourceType { get; set; }
            public string type { get; set; }
            public string title { get; set; }
            public int? numberOfItemsInOrder { get; set; }
            public bool? reopenedOrder { get; set; }
            public List<NavItem> navItems { get; set; }
            public Text text { get; set; }
            public NavItem2 navItem { get; set; }
            public string foldOutChannelId { get; set; }
            public Links _links { get; set; }
            public string context { get; set; }
            public string viewType { get; set; }
            public Embedded3 _embedded { get; set; }
            public string id { get; set; }
            public Link5 link { get; set; }
            public string status { get; set; }
            public bool? loyalty { get; set; }
            public int? memberId { get; set; }
        }

        public class Embedded2
        {
            public List<Item> items { get; set; }
        }

        public class Lane
        {
            public string classDefinition { get; set; }
            public string type { get; set; }
            public Embedded2 _embedded { get; set; }
            public bool? gutter { get; set; }
        }

        public class Embedded
        {
            public List<Lane> lanes { get; set; }
        }

        public class RootObject
        {
            public Meta _meta { get; set; }
            public Embedded _embedded { get; set; }
        }
    }
}