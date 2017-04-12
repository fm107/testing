import { Component, Input } from '@angular/core'
import { OrderBy } from "./orderBy"
import { Format } from "./format"

@Component({
    selector: 'list-sortable',
    templateUrl: './list-sortable.component.html'    
})

export class ListSortableComponent {

    @Input() columns: any[];
    @Input() data: any[];
    @Input() sort: any;

    selectedClass(columnName): string {
        return columnName == this.sort.column ? 'sort-' + this.sort.descending : null;
    }

    changeSorting(columnName): void {
        var sort = this.sort;
        if (sort.column == columnName) {
            sort.descending = !sort.descending;
        } else {
            sort.column = columnName;
            sort.descending = false;
        }
    }

    convertSorting(): string {
        return this.sort.descending ? '-' + this.sort.column : this.sort.column;
    }
}