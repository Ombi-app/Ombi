import { Component, EventEmitter, OnInit, Output } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { BehaviorSubject } from "rxjs";
import { WizardService } from "../services/wizard.service";
import { NotificationService } from "app/services";

@Component({
    templateUrl: "./database.component.html",
    styleUrls: ["../welcome/welcome.component.scss"],
    selector: "wizard-database-selector",
})
export class DatabaseComponent implements OnInit {
    public constructor(private fb: FormBuilder, private service: WizardService, private notification: NotificationService) { }
    @Output() public configuredDatabase = new EventEmitter<void>();

    public form: FormGroup;

    public connectionString = new BehaviorSubject<string>("Server=;Port=3306;Database=ombi");

    public ngOnInit(): void {
        this.form = this.fb.group({
            type: ["MySQL"],
            host: ["", [Validators.required]],
            port: [3306, [Validators.required]],
            name: ["ombi", [Validators.required]],
            user: [""],
            password: [""],
        });

        this.form.valueChanges.subscribe(x => {
            let connection = `Server=${x.host};Port=${x.port};Database=${x.name}`;
            if (x.user) {
                connection = `Server=${x.host};Port=${x.port};Database=${x.name};User=${x.user}`;
                if (x.password) {
                    connection = `Server=${x.host};Port=${x.port};Database=${x.name};User=${x.user};Password=*******`;
                }
            }
            this.connectionString.next(connection);
        });
    }

    public save() {
        this.service.addDatabaseConfig(this.form.value).subscribe(x => {
            this.notification.success(`Database configuration updated! Please now restart ombi!`);
            this.configuredDatabase.emit();
        }, error => {
            this.notification.error(error.error.message);
        })
    }

}
