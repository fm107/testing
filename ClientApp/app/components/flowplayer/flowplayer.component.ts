import { Component, AfterViewInit, ElementRef, Input, ViewChild, NgZone } from '@angular/core';

declare var flowplayer: any;
declare var $: any;
declare var bitmovin: any;

@Component({
    selector: "flowplayer",
    templateUrl: "./flowplayer.component.html"
})

export class FlowplayerComponent {
    @ViewChild("player") player: any;

    // video asset url
    @Input() url: any;

    constructor(public zone: NgZone) {  }

    ngOnInit() {
        this.zone.runOutsideAngular(() => {
            var conf = {
                key: "34dee666-a6a0-4d62-8f1e-3c309e4b7cb6",
                source: {

                    hls: this.url,

                    poster: "https://bitmovin-a.akamaihd.net/content/MI201109210084_1/poster.jpg"
                }
            };
            var player = bitmovin.player("player1");
            player.setup(conf).then(function (value) {
                // Success
                console.log("Successfully created bitmovin player instance");
            }, function (reason) {
                // Error!
                console.log("Error while creating bitmovin player instance");
            });
        });
        

        ////const container = document.getElementById("player"); //this.player.nativeElement; //
        ////console.log(container);
        //const self = $("#player1")[0];
        //console.log(self);
        

        //    // install flowplayer into selected container
        //flowplayer(self,{
        //        splash: true,
        //        overlay: true,
        //        autoplay: true,
        //        clip: {
        //            live: true,
        //            sources: [
        //                //{
        //                //    type: "application/x-mpegurl",
        //                //    src: "http://nextiptv.ddns.net:2580/get.php?username=6085F2CBdb&password=6085F2CBdb&type=m3u"
        //                //},
        //                {
        //                    type:"video/mp4",
        //                    src:"http://techslides.com/demos/sample-videos/small.mp4"
        //                }
        //            ]
        //        }
        //    }).on("ready", (e, api, video) => {

        //        console.log("ready");
        //        // Make up an aspect ratio
        //        var aspectRatio = 364 / 540;

        //        // internal method to handle a window resize event to adjust the video player
        //        function resizeVideoJS() {
        //            var width = document.getElementById("player1").parentElement.offsetWidth;
        //            this.width(width).height(width * aspectRatio);
        //        }

        //        // Initialize resizeVideoJS()
        //        resizeVideoJS();

        //        // Then on resize call resizeVideoJS()
        //        window.onresize = resizeVideoJS;

        //        console.log(this.url);
        //        document.querySelector("player.fp-title").innerHTML =
        //            api.engine.engineName + " engine playing " + video.type;

        //    });
    }

    ngAfterViewInit() {
        

        
    }

    //ngOnChanges() {
    //    flowplayer(document.getElementById("fp-hlsjs"), {
    //        splash: true,
    //        ratio: 9 / 16,

    //        // optional: HLS levels offered for manual selection
    //        hlsQualities: [-1, 1, 3, 6],

    //        clip: {
    //            title: "...", // updated on ready
    //            sources: [
    //                {
    //                    type: "application/x-mpegurl",
    //                    src: this.url
    //                }
    //            ]
    //        },
    //        embed: false
            
    //    }).on("ready", (e, api, video) => {
    //        console.log(this.url);
    //        document.querySelector("fp-hlsjs.fp-title").innerHTML =
    //            api.engine.engineName + " engine playing " + video.type;

    //    });
    //}
}