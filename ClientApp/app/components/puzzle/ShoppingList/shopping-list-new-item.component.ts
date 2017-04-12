import { Component, EventEmitter, Output } from "@angular/core";

import { IListItem } from "./ListItem";
import { shoppingList } from "./mock-shopping-list";
import { ShoppingListService } from "./shopping-list.service";

@Component({
    selector: "shopping-list-new-item",
    templateUrl: "./shopping-list-new-item.component.html"
})

export class ShoppingListNewItemComponent {
    item = { name: '', amount: 0 };
    @Output() itemAdded = new EventEmitter<IListItem>();
    
    constructor(private _shoppingListService: ShoppingListService) {

    }

    onClick() {
        this._shoppingListService.insertItem({ name: this.item.name, amount: this.item.amount});
    }
}