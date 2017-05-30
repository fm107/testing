import { Component, ViewContainerRef, ComponentRef } from "@angular/core";

declare var $: any;
declare var lity: any;

@Component({
    templateUrl: "./invoke-player.component.html"
})

export class InvokePlayerComponent {
    idx: string;
    url: string;
    showVideo: boolean;

    initVideo(component: ComponentRef<{}>, videojs: ViewContainerRef, id: string) {
        $("#video").click();

        $(document).on("lity:close", (event, instance) => {
            console.log("Lightbox closed");
            
            videojs.clear();
            component.destroy(); 
            $(`#video_${id}`).remove();
        });
    }

    initContent(component: ComponentRef<{}>, videojs: ViewContainerRef) {
        lity(this.url);

        $(document).on("lity:close", () => {
            console.log("Lightbox closed");

            videojs.clear();
            component.destroy();
        });
    }
}