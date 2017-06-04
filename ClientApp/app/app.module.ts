import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { UniversalModule } from 'angular2-universal';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { MaterialModule } from '@angular/material';
import { FlexLayoutModule } from '@angular/flex-layout';
import { CovalentCoreModule, TdLoadingService, CovalentDialogsModule } from '@covalent/core';
import { SimpleNotificationsModule } from 'angular2-notifications';
import { SimpleTimer } from 'ng2-simple-timer';

import { AppComponent } from './components/app/app.component'
import { NavMenuComponent } from './components/navmenu/navmenu.component';
import { HomeComponent } from './components/home/home.component';
import { DialogComponent } from './components/dialog/dialog.component';
import { UploadButtonComponent } from "./components/upload-button/upload-button.component";
import { UploadButtonUrlComponent } from "./components/upload-button-url/upload-button-url.component";
import { VideoJSComponent } from "./components/videojs/videojs.component";

import { DataService } from "./services/data.service";
import { WebSocketService } from "./services/websocket.service";
import { DataPresenterComponent } from "./components/data-presenter/data-presenter.component";
import { ContentService } from "./services/content.service";

import { FilterPipe } from "./pipes/filter.pipe";
import { SortPipe } from "./pipes/sort.pipe";
import { FileSizePipe } from "./pipes/filesize.pipe";
import { ShowFilesPipe } from "./pipes/show-files.pipe";
import { InvokePlayerComponent } from "./components/invoke-player/invoke-player.component";

@NgModule({
    bootstrap: [AppComponent],
    declarations: [
        AppComponent,
        NavMenuComponent,
        HomeComponent,
        UploadButtonComponent, UploadButtonUrlComponent, DialogComponent, DataPresenterComponent, VideoJSComponent, InvokePlayerComponent,

        FilterPipe, SortPipe, FileSizePipe, ShowFilesPipe
    ],
    imports: [
        UniversalModule, // Must be first import. This automatically imports BrowserModule, HttpModule, and JsonpModule too.
        FormsModule,
        CommonModule,

        MaterialModule,
        FlexLayoutModule,

        SimpleNotificationsModule.forRoot(),

        CovalentCoreModule,
        CovalentDialogsModule,

        RouterModule.forRoot([
            { path: '', redirectTo: 'home', pathMatch: 'full' },
            { path: 'home', component: HomeComponent },
            { path: '**', redirectTo: 'home' }
        ])
    ],
    entryComponents: [HomeComponent, InvokePlayerComponent],
    providers: [DataService, ContentService, WebSocketService, TdLoadingService, SimpleTimer]
})
export class AppModule {
}
