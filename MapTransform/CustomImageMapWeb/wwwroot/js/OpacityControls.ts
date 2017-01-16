/*
        Leaflet.OpacityControls, a plugin for adjusting the opacity of a Leaflet map.
        (c) 2013, Jared Dominguez
        (c) 2013, LizardTech

        https://github.com/lizardtechblog/Leaflet.OpacityControls
*/

//Declare global variables
var opacity_layer;


//Create a jquery-ui slider with values from 0 to 100. Match the opacity value to the slider value divided by 100.
L.Control.opacitySlider = L.Control.extend({
    options: {
        position: 'topright'
    },
    setOpacityLayer: function (layer) {
        opacity_layer = layer;
    },
    onAdd: function (map) {
        var opacity_slider_div = L.DomUtil.create('div', 'opacity_slider_control');

        $(opacity_slider_div).slider({
            orientation: "vertical",
            range: "min",
            min: 0,
            max: 100,
            value: 60,
            step: 10,
            start: function (event, ui) {
                //When moving the slider, disable panning.
                map.dragging.disable();
                map.once('mousedown', function (e) {
                    map.dragging.enable();
                });
            },
            slide: function (event, ui) {
                var slider_value = ui.value / 100;
                opacity_layer.setOpacity(slider_value);
            }
        });

        return opacity_slider_div;
    }
});