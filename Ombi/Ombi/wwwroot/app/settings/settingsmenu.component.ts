import { Component, OnInit } from '@angular/core';
import { MenuItem } from 'primeng/primeng';
@Component({
    selector: 'settings-menu',
    moduleId: module.id,
    templateUrl: './settingsmenu.component.html'
})
export class SettingsMenuComponent implements OnInit {
    private menu: MenuItem[];

    ngOnInit() {
        this.menu = [{
            label: 'File',
            items: [
                { label: 'Ombi', icon: 'fa-plus', routerLink:"/Settings/Ombi" },
                { label: 'Open', icon: 'fa-download' }
            ]
        },
        {
            label: 'Edit',
            items: [
                { label: 'Undo', icon: 'fa-refresh' },
                { label: 'Redo', icon: 'fa-repeat' }
            ]
        }];
    }
}