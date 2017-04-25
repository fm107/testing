import { Injectable } from "@angular/core";
import { Http, URLSearchParams, Response } from "@angular/http";
import { Observable } from "rxjs/Observable";
import { BehaviorSubject } from "rxjs/BehaviorSubject";
import { AsyncSubject } from "rxjs/AsyncSubject";
import { Subject } from "rxjs/Subject";

import { TdLoadingService, LoadingType, LoadingMode } from "@covalent/core";

import "rxjs/add/operator/map";

import { Headers, RequestOptions } from "@angular/http";

import { HomeComponent } from "../home/home.component";

@Injectable()
export class DataService {
    homeComponent: HomeComponent;

    constructor(private http: Http,
        private loadingService: TdLoadingService) {

        this.loadingService.create({
            name: "query",
            type: LoadingType.Circular,
            mode: LoadingMode.Indeterminate,
            color: "accent"
        });
    }

    getFolderContent(folder: string) {
        const params = new URLSearchParams();
        params.set("folder", folder);

        return Observable.create(observer => {
            this.loadingService.register("query");

            this.http.get(`api/Content/GetFolder`, { search: params }).subscribe(
                result => observer.next(result),
                error => {
                    this.loadingService.resolve("query");
                    observer.error(error);
                    console.error(`Error during retriving data: ${error}`);
                },
                () => {
                    this.loadingService.resolve("query");
                    observer.complete();
                });
        });
    }

    submitTorrentUrl(url: string, folder: string): Observable<Response> {
        const headers = new Headers({ 'Content-Type': "application/json" });
        //headers.append('Access-Control-Allow-Origin', '*');

        const params = new URLSearchParams();
        params.set("url", url);
        params.set("folder", folder);

        const options = new RequestOptions({ search: params, headers: headers });

        this.loadingService.register("query");
        return this.http.post(`api/Torrent/UploadFromUrl`, params.toString(), options)
            .map((response: Response) => {
                return response;
            })
            .catch(error => {
                return Observable.throw(error.json().error || "Server error");
            })
            .finally(() => {
                this.loadingService.resolve("query");
            });
    }
}