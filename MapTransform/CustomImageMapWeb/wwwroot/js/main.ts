declare var $;



class MarkerStore {
    
    private markerPairs: [L.Marker, L.Marker][] = [];


    public add(marker1: L.Marker, marker2: L.Marker) {
        this.markerPairs.push([marker1, marker2]);
    }

    public getMarkerCount() {
        return this.markerPairs.length;
    }

    public getMarkers() {
        return this.markerPairs;
    }
}



class MapWarper {
    private map: L.Map;
    private imgMap: L.Map;

    private imgMapBaseZoom = 13;

    private markerStore: MarkerStore = new MarkerStore();

    constructor(map1PlaceholderId: string, map2PlaceholderId: string) {
        var googleRoadsLayer = L.gridLayer.googleMutant({
            type: 'roadmap' // valid values are 'roadmap', 'satellite', 'terrain' and 'hybrid'
        });;

        let osmLayer = L.tileLayer('http://{s}.tile.osm.org/{z}/{x}/{y}.png', {
            attribution: '&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
        });


        this.map = L
            .map(map1PlaceholderId, {
                layers: [googleRoadsLayer]
            })
            .setView([51.505, -0.09], 13);



        L.control.layers(
            {
                'Google Roads': googleRoadsLayer,
                'OSM': osmLayer
            }, {
            })
            .addTo(this.map);

        // --


        this.imgMap = L
            .map(map2PlaceholderId);

    }


    public loadImage(src: string) {
        var p1 = this.imgMap.unproject([0, 0], this.imgMapBaseZoom);
        var p2 = this.imgMap.unproject([5615, 4042], this.imgMapBaseZoom);


        this
            .imgMap
            .setView([(p2.lat + p1.lat) / 2, (p1.lng + p2.lng) / 2], this.imgMapBaseZoom);
        

        var imageBounds: [number, number][] = [[p1.lat, p1.lng], [p2.lat, p2.lng]];
        console.log(imageBounds);

        L
            .imageOverlay(src, imageBounds)
            .addTo(this.imgMap);

    }


    public addMarker(p1: L.LatLng, p2: L.LatLng)
    {
        let index = this.markerStore.getMarkerCount();
        let char = String.fromCharCode(65 + index);

        var icon = L.icon({
            iconUrl: 'images/markers/blue_Marker' + char + '.png',

            iconSize: [20, 34],
            iconAnchor: [10, 34]
        });

        let marker1 = L
            .marker(p1, {
                draggable: true,
                icon: icon
            })
            .addTo(this.map);


        let marker2 = L
            .marker(p2, {
                draggable: true,
                icon: icon
            })
            .addTo(this.imgMap);

        this.markerStore.add(marker1, marker2);
    }

    public addMarkerToCenter() {
        this.addMarker(this.map.getCenter(), this.imgMap.getCenter());
    }

    public serializeMarkers() {
        let result = [];
        for (let item of this.markerStore.getMarkers()) {
            let p1 = item[0].getLatLng();
            let p2 = item[1].getLatLng();

            let p2Projected = this.imgMapLatLngToXY(p2);

            result.push(p2Projected.x, p2Projected.y, p1.lng, p1.lat);
        }

        return result;
    }

    public imgMapLatLngToXY(latLng: L.LatLng) {
        return this.imgMap.project(latLng, this.imgMapBaseZoom)
    }

    public imgMapXYToLatLng(x: number, y: number): L.LatLng {
        return this.imgMap.unproject([x, y], this.imgMapBaseZoom)
    }
}




class PreviewMap {
    private map: L.Map;
    private overlay: L.ImageOverlay = null;

    private opacitySlider: L.Control.opacitySlider = null;
     
    constructor(placeholderId: string) {

        var googleRoadsLayer = L.gridLayer.googleMutant({
            type: 'roadmap' // valid values are 'roadmap', 'satellite', 'terrain' and 'hybrid'
        });;

        let osmLayer = L.tileLayer('http://{s}.tile.osm.org/{z}/{x}/{y}.png', {
            attribution: '&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
        });
        

        this.map = L
            .map(placeholderId, {
                layers: [googleRoadsLayer]
            })
            .setView([51.505, -0.09], 13);



        L.control.layers(
            {
                'Google Roads': googleRoadsLayer,
                'OSM': osmLayer
            }, {
            })
            .addTo(this.map);

        
        this.opacitySlider = new L.Control.opacitySlider();
        this.map.addControl(this.opacitySlider);

        
    }

    public show(jpgUrl: string, jpgCornerCoordinates: {
        upperLeft: [number, number];
        lowerLeft: [number, number];
        upperRight: [number, number];
        lowerRight: [number, number];
    })
    {
        if (this.overlay != null) {
            this.overlay.removeFrom(this.map);
        }

        var imageBounds: [number, number][] = [[jpgCornerCoordinates.upperLeft[1], jpgCornerCoordinates.upperLeft[0]], [jpgCornerCoordinates.lowerRight[1], jpgCornerCoordinates.lowerRight[0]]];
        
        this.overlay = L
            .imageOverlay(jpgUrl, imageBounds, {
                opacity: .6
            })
            .addTo(this.map);


        this.opacitySlider.setOpacityLayer(this.overlay);
    }
}



let warper = new MapWarper('map1', 'map2');
warper.loadImage('./ordesa.jpg');


let previewMap = new PreviewMap('preview-map');


let points = [
    2182.0480714032 // x
    , 1079.1186600104 // y
    , -0.027731359 // lat
    , 42.683687447 // lng

    , 4836.3574218623
    , 188.9133262387
    , 0.1948544383
    , 42.7325371163

    , 4562.3597533423
    , 3221.43362442239
    , 0.1628556848
    , 42.5478163951

    , 901.54534623294
    , 3603.72123486013
    , -0.1400944591
    , 42.5326387248

    , 2833.7686364819
    , 3896.20743004992
    , 0.0180914998
    , 42.5107194551
];

for (let i = 0; i < points.length; i += 4) {
    warper.addMarker(
        L.latLng(points[i + 3], points[i + 2]),
        L.latLng(warper.imgMapXYToLatLng(points[i], points[i + 1]))
    );
}


/*
console.log('result');
console.log(serializeMarkers());

console.log('target');
console.log(points);
*/




document.getElementById('add-marker').addEventListener('click', (e) => {

    warper.addMarkerToCenter();
});



document.getElementById('preview').addEventListener('click', (e) => {
    let data = new FormData();

    let items = warper.serializeMarkers();
    for (let item of items) {
        data.append('Gcp', item);
    }


    $.ajax({
        url: '/Warp',
        data: data,
        contentType: false,
        processData: false,
        type: 'POST',
        success: function (data) {

            previewMap.show(data.jpgUrl, data.jpgCornerCoordinates);
            console.log(data);
        }
    });
});



let json = '{"jpgUrl":"/result.jpg","jpgCornerCoordinates":{"upperLeft":[-0.216215,42.7543907],"lowerLeft":[-0.216215,42.4956674],"upperRight":[0.2593904,42.7543907],"lowerRight":[0.2593904,42.4956674]}}';


let jsonData = JSON.parse(json);

previewMap.show(jsonData.jpgUrl, jsonData.jpgCornerCoordinates);