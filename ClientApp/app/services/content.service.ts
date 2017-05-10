﻿import { Injectable } from "@angular/core";
import { BehaviorSubject } from "rxjs/BehaviorSubject";
import { SimpleTimer } from 'ng2-simple-timer';

import { IContent } from "../model/content";
import { DataService } from "./data.service";

@Injectable()
export class ContentService {
    metaData: BehaviorSubject<IContent[]>;
    parentFolder: BehaviorSubject<string>;
    currentFolder: BehaviorSubject<string>;

    constructor(private data: DataService) {
        this.metaData = new BehaviorSubject<IContent[]>(null);
        this.parentFolder = new BehaviorSubject<string>(null);
        this.currentFolder = new BehaviorSubject<string>(null);
    }

    getContent(request: string) {
        this.data.getFolderContent(request).subscribe(result => {
            const res = JSON.parse((result)) as IContent[];
            this.metaData.next(res);
            for (let i of res) {
                this.parentFolder.next(i.parentFolder);
                this.currentFolder.next(i.currentFolder);
            }

        });
    }
}