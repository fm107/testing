import { Component } from "@angular/core";

import { TdLoadingService } from "@covalent/core";
import { TdFileUploadComponent } from "@covalent/file-upload";
import { NotificationsService, SimpleNotificationsComponent, PushNotificationsService } from "angular2-notifications";

import { ContentService } from "../../services/content.service";
import { DataService } from "../../services/data.service";

@Component({
    selector: "upload-button",
    templateUrl: "./upload-button.component.html",
    styleUrls: ["./upload-button.component.css"]
})

export class UploadButtonComponent {

    constructor(private service: NotificationsService,
        private content: ContentService,
        private data: DataService,
        private loadingService: TdLoadingService) {
    }

    private selectEvent(file: File, uploadComponent: TdFileUploadComponent): void {
        if (file.type != "application/x-bittorrent") {
            uploadComponent.cancel();

            this.service.error("File Error",
                `No torrents detected in given file`,
                {
                    timeOut: 3000,
                    showProgressBar: true,
                    pauseOnHover: true,
                    clickToClose: true,
                    maxLength: 100
                });
        } else {

            if (file.size > 1048576) {
                uploadComponent.cancel();

                this.service.error("File Size Exceeded",
                    `${file.name} exceeds the limit`,
                    {
                        timeOut: 3000,
                        showProgressBar: true,
                        pauseOnHover: true,
                        clickToClose: true,
                        maxLength: 100
                    });
            }
        }
    };

    private uploadEvent(file: File, uploadComponent: TdFileUploadComponent): void {
            this.data.submitTorrentFile(file, this.content.currentFolder.getValue()).subscribe(response => {
                this.service.success("File Uploaded",
                    `${response} uploaded successfully`,
                    {
                        timeOut: 3000,
                        showProgressBar: true,
                        pauseOnHover: true,
                        clickToClose: true,
                        maxLength: 100
                    });

                this.content.getContent(null, false, null);
            });

        uploadComponent.cancel();
    };
}
