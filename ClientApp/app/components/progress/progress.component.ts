import { Component, OnInit, OnDestroy, ViewContainerRef, ComponentFactoryResolver, Input, ChangeDetectorRef, AfterViewInit, OnChanges } from "@angular/core";
import { Response } from "@angular/http";
import { Subscription } from "rxjs/Subscription";
import { Subject } from "rxjs/Subject";
import { AsyncSubject } from "rxjs/AsyncSubject";
import { Observable } from 'rxjs/Rx';
import { BehaviorSubject } from "rxjs/BehaviorSubject";

import { ClickedItem } from "../data-presenter/ClickedItem";
import { DataService } from "../../services/data.service";
import { IContent } from "../../model/content";
import { TorrentProgressService } from "../../services/torrent-progress.service";
import { ITorrentInfo } from "../../model/torrentInfo";

@Component({
    selector: "torrentProgress",
    templateUrl: "./progress.component.html",
    styleUrls: ["./progress.component.css"]
})
export class ProgressComponent implements OnChanges {
    private isInProgress = new BehaviorSubject<boolean>(null);
    private torrentDetails = new Subject<any>();
    private progressSub: Subscription;
    private torrentDetailsSub: Subscription;

    @Input("Item") item: ClickedItem;

    constructor(private progressService: TorrentProgressService,
                private dataService: DataService) {
    }

    ngOnChanges() {
        if (!this.progressSub) {
            this.progressSub = this.progressService.getUpdates(this.item.hash)
                .subscribe(progress => {
                    this.isInProgress.next(progress);

                    if (!this.torrentDetailsSub) {
                        this.torrentDetailsSub = Observable.timer(0, 5000).subscribe(() => {
                            this.dataService.getTorrentStatus(this.item.hash, `api/Torrent/GetTorrentDetails`).subscribe(
                                (res: Response) => {
                                    const response = JSON.parse(res.text()) as ITorrentInfo;
                                    let details = "";
                                    for (let key of Object.keys(response)) {
                                        details += `${key}: ${response[key]}
                                `;
                                    }
                                    this.torrentDetails.next(details);

                                    if (!progress) {
                                        this.torrentDetailsSub.unsubscribe();
                                    }
                                });
                        });
                    }
                });
        }
    }

    ngOnDestroy() {
        if (this.progressSub) {
            this.progressSub.unsubscribe();
        }
        if (this.torrentDetailsSub) {
            this.torrentDetailsSub.unsubscribe();
        }
    }
}
