import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { UniversalModule } from 'angular2-universal';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common'; 

import { AppComponent } from './components/app/app.component'
import { NavMenuComponent } from './components/navmenu/navmenu.component';
import { HomeComponent } from './components/home/home.component';
import { FetchDataComponent } from './components/fetchdata/fetchdata.component';
import { CounterComponent } from './components/counter/counter.component';
import { UploadComponent } from './components/uploadfile/upload.component';
import { DialogComponent } from './components/dialog/dialog.component';
import { PuzzleComponent } from './components/puzzle/puzzle.component';
import { ShoppingListComponent } from './components/puzzle/ShoppingList/shopping-list.component';
import { ShoppingListNewItemComponent } from './components/puzzle/ShoppingList/shopping-list-new-item.component';
import { ShoppingListItemComponent } from './components/puzzle/ShoppingList/shopping-list-item.component';

import { ButtonModule, FileUploadModule, CheckboxModule, ToolbarModule, SplitButtonModule, GrowlModule, DialogModule, DataTableModule, SharedModule, LightboxModule } from 'primeng/primeng';

import { MaterialModule } from '@angular/material';

import { CovalentCoreModule } from '@covalent/core';
import { CovalentFileModule } from '@covalent/file-upload';

import { SimpleNotificationsModule } from 'angular2-notifications';

import { FileSizePipe } from './pipes/filesize.pipe';



@NgModule({
    bootstrap: [ AppComponent ],
    declarations: [
        AppComponent,
        NavMenuComponent,
        CounterComponent,
        FetchDataComponent,
        HomeComponent,
        UploadComponent, DialogComponent,
        PuzzleComponent, ShoppingListComponent, ShoppingListNewItemComponent, ShoppingListItemComponent, FileSizePipe
    ],
    imports: [
        UniversalModule, // Must be first import. This automatically imports BrowserModule, HttpModule, and JsonpModule too.
        FormsModule,
        CommonModule,

        MaterialModule,

        SimpleNotificationsModule.forRoot(),
        CovalentCoreModule,
        //CovalentFileModule,

        FileUploadModule, ButtonModule, CheckboxModule, ToolbarModule, SplitButtonModule, GrowlModule, DialogModule, DataTableModule, SharedModule, LightboxModule,

        RouterModule.forRoot([
            { path: '', redirectTo: 'home', pathMatch: 'full' },
            { path: 'home', component: HomeComponent },
            { path: 'counter', component: CounterComponent },
            { path: 'fetch-data', component: FetchDataComponent },
            { path: 'upload', component: UploadComponent },
            { path: 'puzzle', component: PuzzleComponent},
            { path: '**', redirectTo: 'home' }
        ])
    ]
})
export class AppModule {
}
