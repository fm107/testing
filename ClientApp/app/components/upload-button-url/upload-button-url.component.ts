import { Component } from "@angular/core";

import { NotificationsService, SimpleNotificationsComponent, PushNotificationsService } from "angular2-notifications";
import { DataService } from "../../services/data.service";

import { Response } from '@angular/http';
import { ContentService } from "../../services/content.service";

@Component({
    selector: "upload-button-url",
    templateUrl: "./upload-button-url.component.html",
    styleUrls: ["./upload-button-url.component.css"]
})
export class UploadButtonUrlComponent {
    constructor(private data: DataService, private content: ContentService,
        private service: NotificationsService) {
    }

    onClick(url: any) {
        this.data.submitTorrentUrl(url.value, this.content.currentFolder.getValue()).subscribe(
            (res: Response) => {
                if (res.status === 200) {
                    this.service.success("File Uploaded",
                        `${res.text()} uploaded successfully`,
                        {
                            timeOut: 5000,
                            showProgressBar: true,
                            pauseOnHover: true,
                            clickToClose: true,
                            maxLength: 100
                        });

                    this.content.getContent(null, false, null);
                }
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
            });

        url.value = "";
    }
}