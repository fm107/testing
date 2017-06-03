import { Injectable } from "@angular/core";
import { Http, URLSearchParams, Response } from "@angular/http";
import { Headers, RequestOptions } from "@angular/http";
import { Observable } from "rxjs/Observable";
import { Subject } from "rxjs/Subject";
import "rxjs/add/operator/map";

import { TdLoadingService, LoadingType, LoadingMode } from "@covalent/core";
import { SimpleTimer } from 'ng2-simple-timer';

@Injectable()
export class DataService {
    folderContent: Subject<any>;

    constructor(private http: Http,
        private loadingService: TdLoadingService,
        private st: SimpleTimer) {

        this.loadingService.create({
            name: "query",
            type: LoadingType.Circular,
            mode: LoadingMode.Indeterminate,
            color: "accent"
        });

        this.folderContent = new Subject<any>();
        this.st.newTimer('5sec', 5);
    }

    getFolderContent(folder: string, needFiles: boolean, hash: string) {
        const params = new URLSearchParams();
        params.set("folder", folder);
        params.set("needFiles", needFiles.toString());
        params.set("hash", hash);

        this.loadingService.register("query");
            this.http.get(`api/Content/GetFolder`, { search: params }).subscribe(
                result => this.folderContent.next(result.text()),
                error => {
                    this.loadingService.resolve("query");
                    this.handleError(error);
                },
                () => {
                    this.loadingService.resolve("query");
                });

        return this.folderContent.asObservable();
    }

    submitTorrentUrl(url: string, folder: string): Observable<Response> {
        const body = new FormData();
        body.append("url", url);
        body.append("folder", folder);

        this.loadingService.register("query");
        return this.http.post(`api/Torrent/UploadFromUrl`, body)
            .map((response: Response) => {
                return response;
            })
            .catch(this.handleError)
            .finally(() => {
                this.loadingService.resolve("query");
            });
    }

    handleError(error) {
        console.error(`Error during retriving data: ${error}`);
        return Observable.throw(error.json().error || "Server error");
    }

    getByTime() {
        this.st.subscribe('5sec', e => console.log("Timer 5sec"));
    }
}