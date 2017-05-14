import { Component, OnInit, ElementRef, Input } from '@angular/core';

declare var videojs: any;

@Component({
    selector: "videojs",
    templateUrl: "./videojs.component.html"
})

export class VideoJSComponent {

    // reference to the element itself, we use this to access events and methods
    private _elementRef: ElementRef;

    // index to create unique ID for component
    @Input() idx: string;

    // video asset url
    @Input() url: any;

    // declare player var
    private player: any;

    // constructor initializes our declared vars
    constructor(elementRef: ElementRef) {
        this.url = false;
        this.player = false;
    }

    // use ngAfterViewInit to make sure we initialize the videojs element
    // after the component template itself has been rendered
    ngAfterViewInit() {
        this.init();
    }

    init() {
        console.log("idx - " + this.idx);
        console.log("url - " + this.url);
        // ID with which to access the template's video element
        let el = `video_${this.idx}`;

        // setup the player via the unique element ID
        this.player = videojs(el);

        this.player = videojs(document.getElementById(el), {}, function () {

            // Store the video object
            var myPlayer = this, id = myPlayer.id();

            // Make up an aspect ratio
            var aspectRatio = 400 / 500;

            // internal method to handle a window resize event to adjust the video player
            function resizeVideoJS() {
                var width = document.getElementById(id).parentElement.offsetWidth;
                myPlayer.width(width).height(width * aspectRatio);
            }

            // Initialize resizeVideoJS()
            resizeVideoJS();

            // Then on resize call resizeVideoJS()
            window.onresize = resizeVideoJS;
        });
    }
}