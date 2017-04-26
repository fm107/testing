import { Component, OnInit, Output } from "@angular/core";
import { Http } from "@angular/http";
import { Subject } from "rxjs/Subject";
import { Observable } from "rxjs/Observable";

import { IContent, IFileSystem } from "./Component";
import { DataService } from "../data-service/data.service";
import { WebSocketService } from "../data-service/websocket.service";
import { IMessage } from "../data-service/message";

declare var lity: any;


import { TdDataTableService, TdDataTableSortingOrder, ITdDataTableSortChangeEvent, ITdDataTableColumn } from "@covalent/core";
import { IPageChangeEvent } from "@covalent/core";

@Component({
    selector: "home",
    templateUrl: "./home.component.html",
    styleUrls: ["./home.component.css"], 
    providers: [TdDataTableService]
})

export class HomeComponent implements OnInit {
    fileSystemContent: IFileSystem[];
    parent: string;
    currentFolder: string;
    filteredData: any[] = this.fileSystemContent;
    searchTerm: string;
    sortBy: string;
    sortOrder: TdDataTableSortingOrder = TdDataTableSortingOrder.Descending;

    private messages: IMessage[] = new Array();

    messagesObs: Subject<IMessage>;

    constructor(private http: Http,
        private data: DataService,
        private wsService: WebSocketService,
        private dataTableService: TdDataTableService) {
    }

    private name: ITdDataTableColumn = {
        name: "name", label: "NAME #", tooltip: "Folder or file name", sortable: true
    }
    private size: ITdDataTableColumn = {
        name: "size", label: "SIZE", tooltip: "Folder or file size", sortable: true, numeric: true, format: v => v.toFixed(2)
    }
    private changed: ITdDataTableColumn = {
        name: "lastChanged", label: "LAST CHANGED", tooltip: "Folder or file last changed date", sortable: true, numeric: true
    }

    ngOnInit() {
        this.getContent("");
        this.data.homeComponent = this;

        this.messagesObs = (this.wsService
            .connect("ws://window.location.hostname:80/api/Torrent/Notifications")
            .map((response: MessageEvent): IMessage => {
                const msg = JSON.parse(response.data);
                return {
                    message: msg.message
                };
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
            this.updateDataTable(null);
        });
    }


    sort(sortEvent: ITdDataTableSortChangeEvent): void {
        this.sortBy = sortEvent.name;
        this.sortOrder = sortEvent.order === TdDataTableSortingOrder.Ascending ?
            TdDataTableSortingOrder.Descending : TdDataTableSortingOrder.Ascending;
        console.log(sortEvent);
        this.updateDataTable("sort");
    }

    search(searchTerm: string): void {
        this.searchTerm = searchTerm;
        this.updateDataTable("filter");
    }


    updateDataTable(action: string): void {
        const newData: any[] = this.fileSystemContent;

        switch (action) {
            case "filter":
                this.filteredData = this.dataTableService.filterData(newData, this.searchTerm, true);
                break;
            case "sort":
                this.filteredData = this.dataTableService.sortData(newData, this.sortBy, this.sortOrder);
                break;
            default:
                this.filteredData = newData;
                break;
        }
    }
}











