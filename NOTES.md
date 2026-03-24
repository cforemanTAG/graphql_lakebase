# Notes

- The synced table is set to be Triggered, not continuous for cost reasons
- The synced table is the source for a materialized view - should be called `geo_shapes` - that has a `geo_polygon` (`geography` type) column and a geospatial index on it
- After the synced table has been updated, a stored procedure that refreshes the materialized view must be called.
    - There's an option to refresh the materialized view online with the `CONCURRENTLY` keyword, but unfortunately this can't be used because the Lakebase instance runs out of disk space in tempdb
    - Can potentially create a new tablespace with more disk space and `SET temp_tablespace TO  '<new_temp_tablespace>'` although I doubt this is feasible
- This project builds a container and then uploads it to AWS Elastic Container Registry where it is used in AWS Elastic Container Service.
- This POC hasn't followed best practices for security. For example:
    - The password for the user that accesses the Lakebase instance is hard-coded in the config instead of using AWS Secrets Manager
    - The security group used by the ECS instance allows all traffic from PL-Amherst-Trused-Networks