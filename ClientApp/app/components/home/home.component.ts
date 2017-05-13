import { Component, OnInit, Output } from "@angular/core";

import { Subject } from "rxjs/Subject";
import { TdDataTableService } from "@covalent/core";

import { IMessage } from "../../services/message";
import { WebSocketService } from "../../services/websocket.service";
import { ContentService } from "../../services/content.service";

declare var lity: any;

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

    private messages: IMessage[] = new Array();

    messagesObs: Subject<IMessage>;

    constructor(private content: ContentService,
        private wsService: WebSocketService,
        private dataTableService: TdDataTableService) {
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

    onClick(item) {
        //this.messagesObs.next({ message: "Test message" });
        if (item.type == "file") {
            console.log(item);
            lity(item.downloadPath +"/out.m3u8");
        }

        this.content.getContent(item.itemName);
    }
}