import { Component, OnInit, ElementRef, Input } from '@angular/core';

declare var $: any;

@Component({
    selector: "invoke",
    templateUrl: "./invoke-player.component.html"
})

export class InvokePlayerComponent {
    idx: string;
    url: any;
    
    constructor() {
        //this.url = "http://d2zihajmogu5jn.cloudfront.net/bipbop-advanced/bipbop_16x9_variant.m3u8";
        //this.idx = "0";

        
    }

    init(component, videojs) {
        $("#video").click();

        $(document).on('lity:close', (event, instance) => {
            console.log('Lightbox closed');
            component.destroy();
            videojs.clear();
        });
    }
}