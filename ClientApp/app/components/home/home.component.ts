import { Component, OnInit, ViewChild, TemplateRef } from "@angular/core";
import { Http } from "@angular/http";

import { IContent, IFileSystem} from "./Component";

declare var lity: any;

@Component({
    selector: "home",
    templateUrl: "./home.component.html",
    styleUrls: ["./home.component.css"]
})
export class HomeComponent implements OnInit {
    collection: IFileSystem[];
    parent: string;

    constructor(private http: Http) {
    }

    ngOnInit() {
        this.getCollection("");
    }

    OnClick(item) {

        if (item.type) {

            if (item.type === "file") {

                lity(item.fullName);

            } else {

                const request = `?folder=${item.fullName}`;
                this.getCollection(request);
            }
        } else {

            const request = `?folder=${item}`;
            this.getCollection(request);
        }
    }

    getCollection(request: string) {
        this.http.get(`/api/content/showfilesystem${request}`).subscribe(result => {
            this.collection = (result.json() as IContent).contents;
            this.parent = (result.json() as IContent).parent;
            console.log(`parent: ${this.parent}`);
        });
    }
}