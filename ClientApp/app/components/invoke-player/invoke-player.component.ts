import { Component, ViewContainerRef } from "@angular/core";

declare var $: any;

@Component({
    templateUrl: "./invoke-player.component.html"
})

export class InvokePlayerComponent {
    idx: string;
    url: any;

    init(component, videojs: ViewContainerRef, id: string) {
        $("#video").click();

        $(document).on("lity:close", (event, instance) => {
            console.log("Lightbox closed");
            
            videojs.clear();
            component.destroy(); 
            $(`#video_${id}`).remove();
        });
    }
}