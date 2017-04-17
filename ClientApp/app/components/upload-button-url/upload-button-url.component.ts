import { Component } from "@angular/core";

import { NotificationsService, SimpleNotificationsComponent, PushNotificationsService } from "angular2-notifications";
import { DataService } from "../data-service/data.service";

import { Response } from '@angular/http';

@Component({
    selector: "upload-button-url",
    templateUrl: "./upload-button-url.component.html",
    styleUrls: ["./upload-button-url.component.css"]
})
export class UploadButtonUrlComponent {
    constructor(private data: DataService,
        private service: NotificationsService) {
    }

    onClick(url: any) {
        let response: string;

        this.data.submitTorrentUrl(url.value, this.data.homeComponent.currentFolder).subscribe(
            (res: Response) => {
                response = res.text();
            },
            error => {
                console.error(`Error while file uploading: ${error}`);
                this.service.error("File Error",
                    `No torrents detected in given URL`,
                    {
                        timeOut: 5000,
                        showProgressBar: true,
                        pauseOnHover: true,
                        clickToClose: true,
                        maxLength: 100
                    });
            },
            () => {
                this.service.success("File Uploaded",
                    `${response} uploaded successfully`,
                    {
                        timeOut: 5000,
                        showProgressBar: true,
                        pauseOnHover: true,
                        clickToClose: true,
                        maxLength: 100
                    });
                this.data.homeComponent.getContent(this.data.homeComponent.currentFolder);
            });

        url.value = "";
    }
}