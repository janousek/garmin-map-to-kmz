var MarkerStore = (function () {
    function MarkerStore() {
        this.markerPairs = [];
    }
    MarkerStore.prototype.add = function (marker1, marker2) {
        this.markerPairs.push([marker1, marker2]);
    };
    MarkerStore.prototype.getMarkerCount = function () {
        return this.markerPairs.length;
    };
    MarkerStore.prototype.getMarkers = function () {
        return this.markerPairs;
    };
    return MarkerStore;
}());
var MapWarper = (function () {
    function MapWarper(map1PlaceholderId, map2PlaceholderId) {
        this.imgMapBaseZoom = 13;
        this.markerStore = new MarkerStore();
        this.map = L
            .map(map1PlaceholderId)
            .setView([51.505, -0.09], 13);
        L.tileLayer('http://{s}.tile.osm.org/{z}/{x}/{y}.png', {
            attribution: '&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
        })
            .addTo(this.map);
        // --
        this.imgMap = L
            .map(map2PlaceholderId);
    }
    MapWarper.prototype.loadImage = function (src) {
        var p1 = this.imgMap.unproject([0, 0], this.imgMapBaseZoom);
        var p2 = this.imgMap.unproject([5615, 4042], this.imgMapBaseZoom);
        this
            .imgMap
            .setView([(p2.lat + p1.lat) / 2, (p1.lng + p2.lng) / 2], this.imgMapBaseZoom);
        var imageBounds = [[p1.lat, p1.lng], [p2.lat, p2.lng]];
        console.log(imageBounds);
        L
            .imageOverlay(src, imageBounds)
            .addTo(this.imgMap);
    };
    MapWarper.prototype.addMarker = function (p1, p2) {
        var index = this.markerStore.getMarkerCount();
        var char = String.fromCharCode(65 + index);
        var icon = L.icon({
            iconUrl: 'images/markers/blue_Marker' + char + '.png',
            iconSize: [20, 34],
            iconAnchor: [10, 34]
        });
        var marker1 = L
            .marker(p1, {
            draggable: true,
            icon: icon
        })
            .addTo(this.map);
        var marker2 = L
            .marker(p2, {
            draggable: true,
            icon: icon
        })
            .addTo(this.imgMap);
        this.markerStore.add(marker1, marker2);
    };
    MapWarper.prototype.addMarkerToCenter = function () {
        this.addMarker(this.map.getCenter(), this.imgMap.getCenter());
    };
    MapWarper.prototype.serializeMarkers = function () {
        var result = [];
        for (var _i = 0, _a = this.markerStore.getMarkers(); _i < _a.length; _i++) {
            var item = _a[_i];
            var p1 = item[0].getLatLng();
            var p2 = item[1].getLatLng();
            var p2Projected = this.imgMapLatLngToXY(p2);
            result.push(p2Projected.x, p2Projected.y, p1.lng, p1.lat);
        }
        return result;
    };
    MapWarper.prototype.imgMapLatLngToXY = function (latLng) {
        return this.imgMap.project(latLng, this.imgMapBaseZoom);
    };
    MapWarper.prototype.imgMapXYToLatLng = function (x, y) {
        return this.imgMap.unproject([x, y], this.imgMapBaseZoom);
    };
    return MapWarper;
}());
var warper = new MapWarper('map1', 'map2');
warper.loadImage('./ordesa.jpg');
/*
map.addEventListener('click', function (e) {
    

    let marker = L
        .marker(e.latlng, {
            draggable: true
        })
        .addTo(map);

});

*/
var points = [
    2182.0480714032 // x
    ,
    1079.1186600104 // y
    ,
    -0.027731359 // lat
    ,
    42.683687447 // lng
    ,
    4836.3574218623,
    188.9133262387,
    0.1948544383,
    42.7325371163,
    4562.3597533423,
    3221.43362442239,
    0.1628556848,
    42.5478163951,
    901.54534623294,
    3603.72123486013,
    -0.1400944591,
    42.5326387248,
    2833.7686364819,
    3896.20743004992,
    0.0180914998,
    42.5107194551
];
for (var i = 0; i < points.length; i += 4) {
    warper.addMarker(L.latLng(points[i + 3], points[i + 2]), L.latLng(warper.imgMapXYToLatLng(points[i], points[i + 1])));
}
/*
console.log('result');
console.log(serializeMarkers());

console.log('target');
console.log(points);
*/
document.getElementById('add-marker').addEventListener('click', function (e) {
    warper.addMarkerToCenter();
});
/*
document.getElementById('create-map').addEventListener('click', (e) => {

    let result = [];
    for (let pair of markerPairs) {

        let latLng = pair[0].getLatLng();
        let xy = imgMap.project(pair[1].getLatLng(), imgMap.getZoom());

        result.push([xy.x, xy.y, latLng.lat, latLng.lng]);
    }

    console.log(result);
});
*/
document.getElementById('preview').addEventListener('click', function (e) {
    var data = new FormData();
    var items = warper.serializeMarkers();
    for (var _i = 0, items_1 = items; _i < items_1.length; _i++) {
        var item = items_1[_i];
        data.append('Gcp', item);
    }
    $.ajax({
        url: '/Warp',
        data: data,
        contentType: false,
        processData: false,
        type: 'POST',
        success: function (data) {
            console.log(data);
        }
    });
});
