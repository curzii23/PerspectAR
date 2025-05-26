# Load required libraries
library(readxl)
library(ARTool)
library(dplyr)

# Normality Check Function
check_normality <- function(data, variable) {
  
  # Shapiro-Wilk Test
  shapiro_test <- shapiro.test(data[[variable]])
  shapiro_p <- shapiro_test$p.value

  
  return(shapiro_p)
}

# Load the data from Excel file using a absolute path
file_path <- "NASA_TLX.xlsx"
df <- read_excel(file_path)

# Convert columns to appropriate types
df$Condition <- factor(df$Condition)
df$Visualization <- factor(df$Visualization)
df$Participant_ID <- factor(df$Participant_ID)

df$MentalDemand <- as.numeric(df$MentalDemand)
df$PhysicalDemand <- as.numeric(df$PhysicalDemand)
df$TemporalDemand <- as.numeric(df$TemporalDemand)
df$Performance <- as.numeric(df$Performance)
df$Effort <- as.numeric(df$Effort)
df$Frustration <- as.numeric(df$Frustration)

# List of dependent variables for ART analysis
art_vars <- c("MentalDemand", "PhysicalDemand", "TemporalDemand", "Performance", "Effort", "Frustration")

# Loop over each dependent variable
for (var in art_vars) {
  
  df[[var]] <- as.numeric(df[[var]])
  
  # Perform the normality check
  p_value <- check_normality(df, var)
  print(paste("Shapiro-Wilk Test p-value for", var, ":", round(p_value, 3)))
  
  # If the p-value is less than 0.05, perform a log transformation
  if (p_value < 0.05) {
    log_var <- paste("Log", var, sep = "")
    df[[log_var]] <- log(df[[var]] + 1)  # Adding 1 to avoid log(0)
    
    # Check normality of the log-transformed variable
    p_log_value <- check_normality(df, log_var)
    print(paste("Shapiro-Wilk Test p-value for", log_var, ":", round(p_log_value, 3)))
    
    # Use the log-transformed variable in the ART analysis
    art_var <- log_var
  } else {
    # Use the original variable in the ART analysis
    art_var <- var
  }
  
  # Perform the ART analysis
  art_result <- art(as.formula(paste(art_var, "~ Condition * Visualization + (1|PairID)")), data = df)
  
  # Perform ANOVA on the ART results
  anova_results <- anova(art_result)
  print(anova_results)
  
  # Calculate partial eta squared
  anova_results$part.eta.sq <- with(anova_results, `F` * `Df` / (`F` * `Df` + `Df.res`))
  print(paste("Partial Eta Squared for", var, ":", round(anova_results$part.eta.sq, 3)))
}

  



