import { Injectable } from "@angular/core";

import { IListItem } from "./ListItem";
import {shoppingList} from "./mock-shopping-list";

@Injectable()
export class ShoppingListService {
    getItems(): Array<IListItem> {
        return shoppingList;
    }

    insertItem(item: IListItem) {
        shoppingList.push(item);
    }

    deleteItem(item: IListItem) {
        shoppingList.splice(shoppingList.indexOf(item), 1);
    }
}