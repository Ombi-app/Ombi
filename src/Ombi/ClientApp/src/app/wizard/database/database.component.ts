import { Component, EventEmitter, OnInit, Output } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { BehaviorSubject } from "rxjs";
import { WizardService } from "../services/wizard.service";
import { NotificationService } from "app/services";
import { MatTabChangeEvent } from "@angular/material/tabs";

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
            type: [""],
            host: ["", [Validators.required]],
            port: [3306, [Validators.required]],
            name: ["ombi", [Validators.required]],
            user: [""],
            password: [""],
        });

        this.form.valueChanges.subscribe(x => {
            console.log(x);
            let connection = `Server=${x.host};Port=${x.port};Database=${x.name}`;

            if (x.user) {
                connection += `;User=${x.user}`;
                if (x.password) {
                    connection += `;Password=*******`;
                }
            }

            if (x.type !== "MySQL") {
                connection = connection.replace("Server", "Host").replace("User", "Username");
            }

            this.connectionString.next(connection);
        });
    }

    public tabChange(event: MatTabChangeEvent) {
        if (event.index === 0) {
            this.form.reset();
        }
        if (event.index === 1) {
            this.form.reset({
                type: "MySQL",
                host: "",
                name: "ombi",
                port: 3306,
            });
            this.form.controls.type.setValue("MySQL");

        }
        if (event.index === 2) {
            this.form.reset({
            type:"Postgres",
            host: "",
            name: "ombi",
            port: 5432,
        });

        }
        this.form.markAllAsTouched();
    }

    public save() {
        this.service.addDatabaseConfig(this.form.value).subscribe({
            next: () => {
                this.notification.success(`Database configuration updated! Please now restart Ombi!`);
                this.configuredDatabase.emit();
            },
            error: error => {
                if (error.error.message) {
                    this.notification.error(error.error.message);
                } else {
                    this.notification.error("Something went wrong, please check the logs");
                }
            },
        });
    }

}
