import { Injectable } from "@angular/core";
import { Http, URLSearchParams } from "@angular/http";
import { Observable } from 'rxjs/Observable';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { AsyncSubject } from 'rxjs/AsyncSubject';
import { Subject } from 'rxjs/Subject';

import 'rxjs/add/operator/map';

import { Headers, RequestOptions } from '@angular/http';

import { HomeComponent } from "../home/home.component";

@Injectable()
export class DataService {
    homeComponent: HomeComponent;

    constructor(private http: Http) {
    }

    getFolderContent(folder: string) {
        const params = new URLSearchParams();
        params.set('folder', folder);

        return Observable.create(observer => {
            this.http.get(`/api/content/showfilesystem`, { search: params }).subscribe(
                result => observer.next(result),
                error => console.error(`Error during retriving data: ${error}`), () => observer.complete());
        });
    }

    submitTorrentUrl(url: string, folder: string) {
        const headers = new Headers({ 'Content-Type': 'application/json' });
        //headers.append('Access-Control-Allow-Origin', '*');

        const params = new URLSearchParams();
        params.set('url', url);
        params.set('folder', folder);

        const options = new RequestOptions({ search: params });

        return this.http.post(`api/Torrent/UploadFileUrl`, params.toString(), options)
            .catch((error: any) => {
                console.error(`Error during submiting torrent Url: ${error}`);
                 return Observable.throw(error);
            });
    }
}