import { Component, OnInit, Output } from "@angular/core";
import { Http } from "@angular/http";
import { Subject } from "rxjs/Subject";
import { Observable } from "rxjs/Observable";

import { IContent, IFileSystem } from "./Component";
import { DataService } from "../data-service/data.service";
import { WebSocketService } from "../data-service/websocket.service";
import { IMessage } from "../data-service/message";

declare var lity: any;

@Component({
    selector: "home",
    templateUrl: "./home.component.html",
    styleUrls: ["./home.component.css"]
})

export class HomeComponent implements OnInit {
    fileSystemContent: IFileSystem[];
    parent: string;
    currentFolder: string;

    private messages: IMessage[] = new Array();

    public messagesObs: Subject<IMessage>;

    constructor(private http: Http,
        private data: DataService,
        private wsService: WebSocketService) {
    }

    ngOnInit() {
        this.getContent("");
        this.data.homeComponent = this;

        this.messagesObs = (this.wsService
            .connect("ws://localhost:60917/api/Torrent/Notifications")
            .map((response: MessageEvent): IMessage => {
                const msg = JSON.parse(response.data);
                return {
                    message: msg.message
                }
            }) as Subject<IMessage>);


        this.messagesObs.subscribe(response => {
            this.messages.push(response);
        });
    }

    OnClick(item) {
        this.messagesObs.next({ message: "Test message" });
        if (item.type) {
            if (item.type == "file") {
                lity(item.fullName);
            } else {
                this.getContent(item.fullName);
            }
        } else {
            this.getContent(item);
        }
    }

    getContent(request: string) {
        this.data.getFolderContent(request).subscribe(result => {
            const res = result.json() as IContent;
            this.fileSystemContent = res.contents;
            this.parent = res.parent;
            this.currentFolder = res.currentFolder;
        });
    }
}