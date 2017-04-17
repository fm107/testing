import { Component, OnInit, Output } from "@angular/core";
import { Http } from "@angular/http";

import { IContent, IFileSystem } from "./Component";
import { DataService } from "../data-service/data.service";

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

    constructor(private http: Http,
        private data: DataService) {
    }

    ngOnInit() {
        this.getContent("");
        this.data.homeComponent = this;
    }

    OnClick(item) {
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