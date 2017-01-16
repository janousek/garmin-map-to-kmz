
declare namespace L {

    export namespace gridLayer {
        export var googleMutant: any;
    }

    export namespace Control {
        export class opacitySlider extends L.Control {
            public setOpacityLayer(layer: L.Layer);
        }
    }
}