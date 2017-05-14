import { Component, OnInit, ViewChild, ViewContainerRef, ComponentFactoryResolver } from "@angular/core";

import { Subject } from "rxjs/Subject";
import { TdDataTableService } from "@covalent/core";


import { IMessage } from "../../services/message";
import { WebSocketService } from "../../services/websocket.service";
import { ContentService } from "../../services/content.service";
import { ClickedItem } from "../data-presenter/ClickedItem";
import { VideoJSComponent } from "../videojs/videojs.component";
import { InvokePlayerComponent } from "../invoke-player/invoke-player.component";
declare var $: any;

@Component({
    selector: "home",
    templateUrl: "./home.component.html",
    styleUrls: ["./home.component.css"],
    providers: [TdDataTableService]
})

export class HomeComponent implements OnInit {
    metaData = this.content.metaData;
    parentFolder = this.content.parentFolder;
    currentFolder = this.content.currentFolder;

    idx: string;
    url: string = " ";

    private messages: IMessage[] = new Array();

    messagesObs: Subject<IMessage>;
    @ViewChild('videojs') videoBox: VideoJSComponent;

    @ViewChild('parent', { read: ViewContainerRef }) parent: ViewContainerRef;
    invokePlayerComponent: any;

    constructor(private content: ContentService,
        private wsService: WebSocketService,
        private dataTableService: TdDataTableService, private componentFactoryResolver: ComponentFactoryResolver) {

        this.invokePlayerComponent = this.componentFactoryResolver.resolveComponentFactory(InvokePlayerComponent);
    }

    ngOnInit() {
        this.content.getContent("");

        //this.messagesObs = (this.wsService
        //    .connect(`ws://${window.location.hostname}:${window.location.port}/api/Torrent/Notifications`)
        //    .map((response: MessageEvent): IMessage => {
        //        const msg = JSON.parse(response.data);
        //        return {
        //            message: msg.message
        //        };
        //    }) as Subject<IMessage>);


        //this.messagesObs.subscribe(response => {
        //    console.log(response);
        //    this.messages.push(response);
        //});
    }

    onClick(item: ClickedItem) {
        //this.messagesObs.next({ message: "Test message" });

        //if (item.type == "file") {
        //    this.idx = String(item.id);
        //    this.url = item.downloadPath + "/out.m3u8";

        //    this.videoBox.idx="0";
        //    this.videoBox.url = "http://d2zihajmogu5jn.cloudfront.net/bipbop-advanced/bipbop_16x9_variant.m3u8";
        //    this.videoBox.init();
        //    $("#video").click();
        //} else {
        //    this.content.getContent(item.itemName);
        //}

        //this.childComponent.idx = 
        //this.childComponent.url="http://d2zihajmogu5jn.cloudfront.net/bipbop-advanced/bipbop_16x9_variant.m3u8";

        if (item.type == "file") {
            const cmp = this.parent.createComponent(this.invokePlayerComponent);
            const inv = (cmp.instance) as InvokePlayerComponent;
            inv.url = item.downloadPath + "/out.m3u8";
            inv.idx = String(item.id);
            inv.init(cmp);
        }
        this.content.getContent(item.itemName);
    }
}
