export class Order {
    public id: number
    public dateCreated: Date
    public amount: number

    constructor(id: number, amount: number, dateCreated: Date) { 
        this.id = id;
        this.amount = amount;
        this.dateCreated = dateCreated;
    }
}