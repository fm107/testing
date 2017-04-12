import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { OrderBy } from "./orderBy";
import { Format } from "./format";
import { ListSortableComponent } from './list-sortable.component'

@NgModule({
    imports: [
        CommonModule],

    declarations: [
        ListSortableComponent,
        OrderBy,
        Format
    ],
    exports: [ListSortableComponent]
})
export class ListSortableModule { }